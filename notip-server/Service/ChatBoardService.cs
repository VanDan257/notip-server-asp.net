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

namespace notip_server.Service
{
    public class ChatBoardService : IChatBoardService
    {
        private readonly DbChatContext chatContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private IHubContext<ChatHub> chatHub;
        private readonly IUserService _userService;
        //private readonly string _storageConnectionString;
        //private readonly string _storageContainerName;

        public ChatBoardService(DbChatContext chatContext, IWebHostEnvironment webHostEnvironment, IHubContext<ChatHub> chatHub, IConfiguration configuration, IUserService userService)
        {
            this.chatContext = chatContext;
            this.chatHub = chatHub;
            this.webHostEnvironment = webHostEnvironment;
            _userService = userService;
            //_storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            //_storageContainerName = configuration.GetValue<string>("BlobContainerName");
        }


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

            List<UserDto> users = await _userService.SearchContact(userCode, keySearch);
            if(users.Count > 0)
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
        public async Task<List<MessageDto>> AccessChatGroup(string userCode, string groupCode)
        {
            Group grp = await chatContext.Groups
                .FirstOrDefaultAsync(x => x.Code == groupCode);

            if(grp != null)
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
                            Avatar = x.UserCreatedBy.Avatar
                        }
                    }).ToListAsync();
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

                return null;
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
                // Có thể tối ưu bằng cách chỉ gửi cho user trong nhóm chat
               await chatHub.Clients.All.SendAsync("messageHubListener", true);
            }
            catch (Exception ex)
            {
                //throw new Exception()
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
