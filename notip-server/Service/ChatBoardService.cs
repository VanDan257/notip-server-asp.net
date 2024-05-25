using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using notip_server.Data;
using notip_server.Dto;
using notip_server.Hubs;
using notip_server.Models;
using notip_server.Interfaces;
using notip_server.Utils;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;
using notip_server.ViewModel.ChatBoard;
using System.Numerics;
using System.Net;
using System.Reflection;
using static notip_server.Utils.Constants;
using Newtonsoft.Json;
using notip_server.ViewModel.Friend;
using notip_server.ViewModel.User;
using System.Net.WebSockets;

namespace notip_server.Service
{
    public class ChatBoardService : IChatBoardService
    {
        #region fields
        private readonly DbChatContext chatContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private ChatHub chatHub;
        private readonly IUserService _userService;
        //private readonly string _storageConnectionString;
        //private readonly string _storageContainerName;

        #endregion

        #region ctor
        public ChatBoardService(DbChatContext chatContext, IWebHostEnvironment webHostEnvironment, ChatHub chatHub, IConfiguration configuration, IUserService userService)
        {
            this.chatContext = chatContext;
            this.chatHub = chatHub;
            this.webHostEnvironment = webHostEnvironment;
            _userService = userService;
            //_storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            //_storageContainerName = configuration.GetValue<string>("BlobContainerName");
        }

        #endregion

        /// <summary>
        /// Danh sách lịch sử chat
        /// </summary>
        /// <param name="userSession">User hiện tại đang đăng nhập</param>
        /// <returns>Danh sách lịch sử chat</returns>
        public async Task<List<GroupDto>> GetHistory(string userSession)
        {
            //Lấy danh sách nhóm chat
            List<GroupDto> groups = await chatContext.Groups
                    .Where(x => x.GroupUsers.Any(y => y.UserCode.Equals(userSession)))
                    .Select(x => new GroupDto()
                    {
                        Code = x.Code,
                        Name = x.Name,
                        Avatar = x.Avatar,
                        Type = x.Type,
                        LastActive = x.LastActive,
                        Users = x.GroupUsers.Select(y => new UserDto()
                        {
                            Code = y.User.Code,
                            FullName = y.User.FullName,
                            Avatar = y.User.Avatar,
                        }).ToList(),
                    }).ToListAsync();

            foreach (var group in groups)
            {
                //Nếu nhóm chat có type = SINGLE (chat 1-1) => đổi tên nhóm chat thành tên người chat cùng
                if (group.Type == Constants.GroupType.SINGLE)
                {
                    var us = group.Users.FirstOrDefault(x => !x.Code.Equals(userSession));
                    group.Name = us?.FullName;
                    group.Avatar = us?.Avatar;
                }

                // Lấy tin nhắn gần nhất để hiển thị
                group.LastMessage = await chatContext.Messages
                    .Where(x => x.GroupCode.Equals(group.Code))
                    .OrderByDescending(x => x.Created)
                    .Select(x => new MessageDto()
                    {
                        Created = x.Created,
                        CreatedBy = x.CreatedBy,
                        Content = x.Content,
                        GroupCode = x.GroupCode,
                        Type = x.Type,
                    })
                    .FirstOrDefaultAsync();
            }


            return groups.OrderByDescending(x => x.LastActive).ToList();
        }

        /// <summary>
        /// Tìm kiếm nhóm chat
        /// Đầu tiên sẽ lấy danh sách nhóm chat theo keySearch, sau đó lấy danh sách tên user theo keySearch
        /// Nếu user và user hiện tại đã từng nhắn tin riêng sẽ lấy Code của Group theo Group chat riêng
        /// Nếu không sẽ trả về Code của user
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="keySearch">Tên nhóm hoặc tên người dùng cần tìm</param>
        /// <returns></returns>
        public async Task<List<GroupDto>> SearchChatGroup(string userCode, string keySearch)
        {
            List<GroupDto> groups = await chatContext.Groups
                    .Where(x => x.GroupUsers.Any(y => y.UserCode.Equals(userCode)) && x.Name.Contains(keySearch))
                    .Select(x => new GroupDto()
                    {
                        Code = x.Code,
                        Name = x.Name,
                        Avatar = x.Avatar,
                        Type = x.Type,
                        LastActive = x.LastActive,
                        Users = x.GroupUsers.Select(y => new UserDto()
                        {
                            Code = y.User.Code,
                            FullName = y.User.FullName,
                            Avatar = y.User.Avatar,
                        }).ToList(),
                    }).ToListAsync();

            var requestContact = new GetContactRequest
            {
                KeySearch = keySearch,
                PageSize = 8
            };
            var result = await _userService.GetContact(userCode, requestContact);
            List<FriendResponse> users = result.Items;
            if (users.Count > 0)
            {
                for(int i = 0; i < users.Count; i++)
                {
                    string groupCode = await chatContext.Groups
                        .Where(x => x.Type.Equals(Constants.GroupType.SINGLE))
                        .Where(x => x.GroupUsers.Any(y => y.UserCode.Equals(userCode) &&
                                    x.GroupUsers.Any(y => y.UserCode.Equals(users[i].Code))))
                        .Select(x => x.Code)
                        .FirstOrDefaultAsync();
                    if (!string.IsNullOrEmpty(groupCode))
                    {
                        groups.Add(new GroupDto
                        {
                            Code = groupCode,
                            Type = Constants.GroupType.SINGLE,
                            Avatar = users[i].Avatar,
                            Name = users[i].FullName,
                        });
                    }
                    else
                    {
                        groups.Add(new GroupDto
                        {
                            Code = users[i].Code,
                            Type = Constants.GroupType.SINGLE,
                            Avatar = users[i].Avatar,
                            Name = users[i].FullName,
                        });
                    }
                }
            }

            return groups;
        }

        /// <summary>
        /// - Nếu tồn tại group có code = groupCode sẽ truy cập group đó, 
        /// - Nếu không sẽ tìm kiếm user có code = groupCode và tạo nhóm chat riêng tư
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="groupCode">Code của group muốn truy cập</param>
        /// <returns></returns>
        public async Task<GroupDto> AccessChatGroup(string userCode, string groupCode)
        {
            Group grp = await chatContext.Groups
                .FirstOrDefaultAsync(x => x.Code == groupCode);

            if(grp != null)
            {
                var group = new GroupDto
                {
                    Code = grp.Code,
                    Type = grp.Type,
                    Avatar = grp.Avatar,
                    Name = grp.Name,
                    Created = grp.Created,
                    CreatedBy = grp.CreatedBy,
                    LastActive = grp.LastActive,
                };

                if (grp.Type == GroupType.SINGLE)
                {
                    var partner = await chatContext.GroupUsers.Where(x => x.GroupCode == groupCode && x.UserCode != userCode)
                        .Join(chatContext.Users,
                            grpUsers => grpUsers.UserCode,
                            users => users.Code,
                            (grpUsers, users) => new UserDto
                            {
                                Code = users.Code,
                                FullName = users.FullName,
                                Dob = users.Dob,
                                Phone = users.Phone,
                                Email = users.Email,
                                Address = users.Address,
                                Avatar = users.Avatar,
                                Gender = users.Gender
                            })
                        .FirstOrDefaultAsync();

                    group.Avatar = partner.Avatar;
                    group.Name = partner.FullName;
                }
                else
                {
                    var groupUsers = chatContext.GroupUsers
                        .Where(x => x.GroupCode == groupCode)
                        .Join(chatContext.Users,
                            grpUsers => grpUsers.UserCode,
                            users => users.Code,
                            (grpUsers, users) => new UserDto
                            {
                                Code = users.Code,
                                FullName = users.FullName,
                                Dob = users.Dob,
                                Phone = users.Phone,
                                Email = users.Email,
                                Address = users.Address,
                                Avatar = users.Avatar,
                                Gender = users.Gender
                            })
                        .ToList();

                    group.Users = groupUsers;
                }

                return group;

                //return response;
            }
            else
            {

                // Nếu nhóm không tồn tại => cố gắng lấy thông tin nhóm đã từng chat giữa 2 người
                string grpCode = await chatContext.Groups
                    .Where(x => x.Type.Equals(Constants.GroupType.SINGLE))
                    .Where(x => x.GroupUsers.Any(y => y.UserCode.Equals(userCode) &&
                                x.GroupUsers.Any(y => y.UserCode.Equals(groupCode))))
                    .Select(x => x.Code)
                    .FirstOrDefaultAsync();

                grp = await chatContext.Groups.FirstOrDefaultAsync(x => x.Code.Equals(grpCode));

                if(grp != null)
                {
                    var partner = await chatContext.GroupUsers.Where(x => x.GroupCode == grp.Code && x.UserCode != userCode)
                        .Join(chatContext.Users,
                            grpUsers => grpUsers.UserCode,
                            users => users.Code,
                            (grpUsers, users) => new UserDto
                            {
                                Code = users.Code,
                                FullName = users.FullName,
                                Dob = users.Dob,
                                Phone = users.Phone,
                                Email = users.Email,
                                Address = users.Address,
                                Avatar = users.Avatar,
                                Gender = users.Gender
                            })
                        .FirstOrDefaultAsync();

                    return new GroupDto
                    {
                        Code = grp.Code,
                        Type = grp.Type,
                        Avatar = partner.Avatar,
                        Name = partner.FullName,
                        Created = grp.Created,
                        CreatedBy = grp.CreatedBy,
                        LastActive = grp.LastActive,
                    };
                }
                else
                {
                    Group newGroup = new Group
                    {
                        Code = Guid.NewGuid().ToString("N"),
                        Created = DateTime.Now,
                        CreatedBy = userCode,
                        Type = Constants.GroupType.SINGLE,
                        LastActive = DateTime.Now
                    };

                    User receiver = await chatContext.Users.FirstOrDefaultAsync(x => x.Code == groupCode);

                    newGroup.GroupUsers = new List<GroupUser>();
                    newGroup.GroupUsers.Add(new GroupUser
                    {
                        GroupCode = newGroup.Code,
                        UserCode = groupCode
                    });

                    newGroup.GroupUsers.Add(new GroupUser
                    {
                        GroupCode = newGroup.Code,
                        UserCode = userCode
                    });

                    await chatContext.Groups.AddAsync(newGroup);
                    await chatContext.SaveChangesAsync();

                    return new GroupDto
                    {
                        Code = newGroup.Code,
                        Type = newGroup.Type,
                        Avatar = receiver.Avatar,
                        Name = receiver.FullName,
                        Created = newGroup.Created,
                        CreatedBy = newGroup.CreatedBy,
                        LastActive = newGroup.LastActive,
                    };
                }

            }
        }

        /// <summary>
        /// Thông tin nhóm chat
        /// </summary>
        /// <param name="userSession">User hiện tại đang đăng nhập</param>
        /// <param name="groupCode">Mã nhóm</param>
        /// <param name="contactCode">Người chat</param>
        /// <returns></returns>
        public async Task<object> GetInfo(string userSession, string groupCode)
        {
            //Lấy thông tin nhóm chat
            Group group = await chatContext.Groups.FirstOrDefaultAsync(x => x.Code.Equals(groupCode));

            if(group == null)
            {
                throw new Exception("Không tồn tại nhóm chat");
            }
            // Nếu tồn tại nhóm chat + nhóm chat có type = SINGLE (Chat 1-1) => trả về thông tin người chat cùng
            if (group.Type.Equals(Constants.GroupType.SINGLE))
            {
                string userCode = group.GroupUsers.FirstOrDefault(x => x.UserCode != userSession)?.UserCode;
                return await chatContext.Users
                        .Where(x => x.Code.Equals(userCode))
                        .OrderBy(x => x.FullName)
                        .Select(x => new
                        {
                            IsGroup = false,
                            Code = x.Code,
                            Address = x.Address,
                            Avatar = x.Avatar,
                            Dob = x.Dob,
                            Email = x.Email,
                            FullName = x.FullName,
                            Gender = x.Gender,
                            Phone = x.Phone
                        })
                         .FirstOrDefaultAsync();
            }
            else
            {
                // Nếu tồn tại nhóm chat + nhóm chat nhiều người => trả về thông tin nhóm chat + thành viên trong nhóm
                return new
                {
                    IsGroup = true,
                    Code = group.Code,
                    Avatar = group.Avatar,
                    Name = group.Name,
                    Type = group.Type,
                    Users = group.GroupUsers
                        .OrderBy(x => x.User.FullName)
                        .Select(x => new UserDto()
                        {
                            Code = x.User.Code,
                            FullName = x.User.FullName,
                            Avatar = x.User.Avatar
                        }).ToList()
                };
            }
        }

        /// <summary>
        /// Thêm mới nhóm chat
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="group">Nhóm</param>
        public async Task AddGroup(string userCode, AddGroupRequest request)
        {
            DateTime dateNow = DateTime.Now;
            Group grp = new Group()
            {
                Code = Guid.NewGuid().ToString("N"),
                Name = request.Name,
                Created = dateNow,
                CreatedBy = userCode,
                Type = Constants.GroupType.MULTI,
                LastActive = dateNow,
                Avatar = Constants.AVATAR_DEFAULT
            };

            List<GroupUser> groupUsers = request.Users.Select(x => new GroupUser()
            {
                UserCode = x.Code
            }).ToList();

            groupUsers.Add(new GroupUser()
            {
                UserCode = userCode
            });

            grp.GroupUsers = groupUsers;

            await chatContext.Groups.AddAsync(grp);
            await chatContext.SaveChangesAsync();
        }

        /// <summary>
        /// Thêm thành viên vào nhóm
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task AddMembersToGroup(AddMembersToGroupRequest request)
        {
            try
            {
                var group = await chatContext.Groups.FindAsync(request.Code);

                if(group == null)
                {
                    throw new Exception("Không tìm thấy nhóm");
                }

                List<GroupUser> lstGroupUser = new List<GroupUser>();
                for(int i = 0; i < request.Users.Count; i++)
                {
                    lstGroupUser.Add(new GroupUser
                    {
                        GroupCode = request.Code,
                        UserCode = request.Users[i].Code
                    });
                }

                await chatContext.GroupUsers.AddRangeAsync(lstGroupUser);
                await chatContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Có lỗi xảy ra: " + ex.Message);
            }
        }

        /// <summary>
        /// Rời nhóm
        /// </summary>
        /// <param name="userSession"></param>
        /// <param name="groupCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task OutGroup(string userSession, string groupCode)
        {
            try
            {
                var groupUser = await chatContext.GroupUsers.FirstOrDefaultAsync(x => x.UserCode == userSession && x.GroupCode == groupCode);
                if(groupUser == null)
                {
                    throw new Exception("Nhóm chat không tồn tại");
                }

                chatContext.GroupUsers.Remove(groupUser);
                await chatContext.SaveChangesAsync();
            }
            catch
            {
                throw new Exception("Có lỗi xảy ra");
            }
        }

        /// <summary>
        /// Cập nhật tên nhóm
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdateGroupName(UpdateGroupNameRequest request)
        {
            try
            {
                Group grp = await chatContext.Groups
                    .FirstOrDefaultAsync(x => x.Code == request.Code);
                if( grp == null)
                {
                    throw new Exception("Không tìm thấy nhóm chat");
                }

                grp.Name = request.Name;
                chatContext.Groups.Update(grp);

                await chatContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Có lỗi xảy ra: " + ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật ảnh đại diện của nhóm chat
        /// </summary>
        /// <param name="group">Nhóm</param>
        /// <returns></returns>
        public async Task UpdateGroupAvatar(UpdateGroupAvatarRequest request)
        {
            try
            {
                Group grp = await chatContext.Groups
                    .FirstOrDefaultAsync(x => x.Code == request.Code);

                if (grp != null)
                {
                    string path = Path.Combine(webHostEnvironment.ContentRootPath, $"wwwroot/images/groups/");
                    FileHelper.CreateDirectory(path);
                    string pathFile = path + request.Image[0].FileName;
                    if (!File.Exists(pathFile))
                    {

                        using (var stream = new FileStream(pathFile, FileMode.Create))
                        {
                            await request.Image[0].CopyToAsync(stream);
                            //await UploadBlobFile(message.Attachments[0]);
                        }
                    }
                    grp.Avatar = $"images/groups/{request.Image[0].FileName}";

                    chatContext.Groups.Update(grp);
                    await chatContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Có lỗi xảy ra: " + ex.Message);
            }
        }

        //public async Task UploadBlobFile(IFormFile blob)
        //{
        //    BlobContainerClient container = new BlobContainerClient(_storageConnectionString, _storageContainerName);

        //    // Create the container if it does not exist
        //    await container.CreateIfNotExistsAsync();
        //    BlobClient client = container.GetBlobClient(blob.FileName);

        //    // Open a stream for the file we want to upload
        //    await using (Stream data = blob.OpenReadStream())
        //    {
        //        // Upload the file async
        //        await client.UploadAsync(data);
        //    }


        //}

        /// <summary>
        /// Gửi tin nhắn
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="groupCode">Mã nhóm</param>
        /// <param name="message">Tin nhắn</param>
        public async Task SendMessage(string userCode, string groupCode, MessageDto message)
        {
            // Lấy thông tin nhóm chat
            Group grp = await chatContext.Groups.FirstOrDefaultAsync(x => x.Code.Equals(groupCode));
            DateTime dateNow = DateTime.Now;

            // Nếu nhóm không tồn tại => cố gắng lấy thông tin nhóm đã từng chat giữa 2 người
            if (grp == null)
            {
                string grpCode = await chatContext.Groups
                    .Where(x => x.Type.Equals(Constants.GroupType.SINGLE))
                    .Where(x => x.GroupUsers.Any(y => y.UserCode.Equals(userCode) &&
                                x.GroupUsers.Any(y => y.UserCode.Equals(message.SendTo))))
                    .Select(x => x.Code)
                    .FirstOrDefaultAsync();

                grp = await chatContext.Groups.FirstOrDefaultAsync(x => x.Code.Equals(grpCode));
            }

            // Nếu nhóm vẫn không tồn tại => tạo nhóm chat mới có 2 thành viên
            if (grp == null)
            {
                User sendTo = await chatContext.Users.FirstOrDefaultAsync(x => x.Code.Equals(message.SendTo));
                grp = new Group()
                {
                    Code = Guid.NewGuid().ToString("N"),
                    Name = sendTo.FullName,
                    Created = dateNow,
                    CreatedBy = userCode,
                    Type = Constants.GroupType.SINGLE,
                    GroupUsers = new List<GroupUser>()
                    {
                        new GroupUser()
                        {
                            UserCode = userCode
                        },
                        new GroupUser()
                        {
                            UserCode = sendTo.Code
                        }
                    }
                };
                await chatContext.Groups.AddAsync(grp);
            }

            // Nếu tin nhắn có file => lưu file
            if (message.Attachments != null && message.Attachments.Count > 0)
            {
                string path = Path.Combine(webHostEnvironment.ContentRootPath, $"wwwroot/Attachments/{groupCode}/{DateTime.Now.Year}/");
                FileHelper.CreateDirectory(path);
                try
                {
                    if (message.Attachments[0].Length > 0)
                    {
                        string pathFile = path + message.Attachments[0].FileName;
                        if (!File.Exists(pathFile))
                        {
                            using (var stream = new FileStream(pathFile, FileMode.Create))
                            {
                                await message.Attachments[0].CopyToAsync(stream);
                                //await UploadBlobFile(message.Attachments[0]);
                            }
                        }
                        message.Path = $"Attachments/{groupCode}/{DateTime.Now.Year}/{message.Attachments[0].FileName}";
                        message.Content = message.Attachments[0].FileName;
                        //message.Path = $"https://pnchatstorage.blob.core.windows.net/{_storageContainerName}/{message.Attachments[0].FileName}";
                    }
                }
                catch (Exception ex)
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    throw;
                }
            }

            Message msg = new Message()
            {
                Content = message.Content,
                Created = dateNow,
                CreatedBy = userCode,
                GroupCode = grp.Code,
                Path = message.Path,
                Type = message.Type,
            };

            grp.LastActive = dateNow;

            await chatContext.Messages.AddAsync(msg);
            await chatContext.SaveChangesAsync();
            try
            {
                var groupUsers = chatContext.GroupUsers
                        .Where(x => x.GroupCode == groupCode && x.UserCode != userCode)
                        .Join(chatContext.Users,
                            grpUsers => grpUsers.UserCode,
                            users => users.Code,
                            (grpUsers, users) => 
                                users.Code
                            )
                        .ToList();

                var userCreatedBy = await chatContext.Users
                    .Where(x => x.Code == userCode)
                    .Select(x => new UserDto
                    {
                        FullName = x.FullName,
                        Avatar = x.Avatar
                    })
                    .FirstOrDefaultAsync();

                var messageDto = new MessageDto
                {
                    Id = msg.Id,
                    Type = msg.Type,
                    GroupCode = msg.GroupCode,
                    Content = msg.Content,
                    Path = msg.Path,
                    Created = msg.Created,
                    CreatedBy = msg.CreatedBy,
                    UserCreatedBy = userCreatedBy
                };
                var payload = JsonConvert.SerializeObject(messageDto);
                await chatHub.SendMessage(groupUsers, payload);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách tin nhắn từ nhóm
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="groupCode">Mã nhóm</param>
        /// <returns>Danh sách tin nhắn</returns>
        public async Task<List<MessageDto>> GetMessageByGroup(string userCode, string groupCode)
        {
            return await chatContext.Messages
                    .Where(x => x.GroupCode.Equals(groupCode))
                    .Where(x => x.Group.GroupUsers.Any(y => y.UserCode.Equals(userCode)))
                    .OrderBy(x => x.Created)
                    .Select(x => new MessageDto()
                    {
                        Created = x.Created,
                        Content = x.Content,
                        CreatedBy = x.CreatedBy,
                        GroupCode = x.GroupCode,
                        Id = x.Id,
                        Path = x.Path,
                        Type = x.Type,
                        UserCreatedBy = new UserDto()
                        {
                            Code = x.UserCreatedBy.Code,
                            FullName = x.UserCreatedBy.FullName,
                            Dob = x.UserCreatedBy.Dob,
                            Phone = x.UserCreatedBy.Phone,
                            Email = x.UserCreatedBy.Email,
                            Address = x.UserCreatedBy.Address,
                            Gender = x.UserCreatedBy.Gender,
                            Avatar = x.UserCreatedBy.Avatar

                        }
                    }).ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách tin nhắn với người đã liên hệ
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="contactCode">Người nhắn cùng</param>
        /// <returns></returns>
        public async Task<List<MessageDto>> GetMessageByContact(string userCode, string contactCode)
        {
            // Lấy mã nhóm đã từng nhắn tin giữa 2 người
            string groupCode = await chatContext.Groups
                    .Where(x => x.Type.Equals(Constants.GroupType.SINGLE))
                    .Where(x => x.GroupUsers.Any(y => y.UserCode.Equals(userCode) &&
                                x.GroupUsers.Any(y => y.UserCode.Equals(contactCode))))
                    .Select(x => x.Code)
                    .FirstOrDefaultAsync();

            return await chatContext.Messages
                    .Where(x => x.GroupCode.Equals(groupCode))
                    .Where(x => x.Group.GroupUsers.Any(y => y.UserCode.Equals(userCode)))
                    .OrderBy(x => x.Created)
                    .Select(x => new MessageDto()
                    {
                        Created = x.Created,
                        Content = x.Content,
                        CreatedBy = x.CreatedBy,
                        GroupCode = x.GroupCode,
                        Id = x.Id,
                        Path = x.Path,
                        Type = x.Type,
                        UserCreatedBy = new UserDto()
                        {
                            Avatar = x.UserCreatedBy.Avatar
                        }
                    }).ToListAsync();
        }
    }
}
