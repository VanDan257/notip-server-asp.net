using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using notip_server.Data;
using notip_server.Dto;
using notip_server.Hubs;
using notip_server.Models;
using notip_server.Interfaces;
using notip_server.Utils;
using System.Runtime.ExceptionServices;

namespace notip_server.Service
{
    public class ChatBoardService : IChatBoardService
    {
        protected readonly DbChatContext chatContext;
        protected readonly IWebHostEnvironment webHostEnvironment;
        protected IHubContext<ChatHub> chatHub;
        //private readonly string _storageConnectionString;
        //private readonly string _storageContainerName;

        public ChatBoardService(DbChatContext chatContext, IWebHostEnvironment webHostEnvironment, IHubContext<ChatHub> chatHub, IConfiguration configuration)
        {
            this.chatContext = chatContext;
            this.chatHub = chatHub;
            this.webHostEnvironment = webHostEnvironment;
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
        /// Thông tin nhóm chat
        /// </summary>
        /// <param name="userSession">User hiện tại đang đăng nhập</param>
        /// <param name="groupCode">Mã nhóm</param>
        /// <param name="contactCode">Người chat</param>
        /// <returns></returns>
        public async Task<object> GetInfo(string userSession, string groupCode, string contactCode)
        {
            //Lấy thông tin nhóm chat
            Group group = await chatContext.Groups.FirstOrDefaultAsync(x => x.Code.Equals(groupCode));

            // nếu mã nhóm k tồn tại => thuộc loại chat 1-1 (Tự quy định) => Trả về thông tin người chat cùng
            if (group == null)
            {
                return await chatContext.Users
                        .Where(x => x.Code.Equals(contactCode))
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
                            Phone = x.Phone,
                        })
                        .FirstOrDefaultAsync();
            }
            else
            {
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
        }

        /// <summary>
        /// Thêm mới nhóm chat
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="group">Nhóm</param>
        public async Task AddGroup(string userCode, GroupDto group)
        {
            DateTime dateNow = DateTime.Now;
            Group grp = new Group()
            {
                Code = Guid.NewGuid().ToString("N"),
                Name = group.Name,
                Created = dateNow,
                CreatedBy = userCode,
                Type = Constants.GroupType.MULTI,
                LastActive = dateNow,
                Avatar = Constants.AVATAR_DEFAULT
            };

            List<GroupUser> groupUsers = group.Users.Select(x => new GroupUser()
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
        /// Cập nhật ảnh đại diện của nhóm chat
        /// </summary>
        /// <param name="group">Nhóm</param>
        /// <returns></returns>
        public async Task<GroupDto> UpdateGroupAvatar(GroupDto group)
        {
            Group grp = await chatContext.Groups
                .FirstOrDefaultAsync(x => x.Code == group.Code);

            if (grp != null)
            {
                if (group.Avatar.Contains("data:image/png;base64,"))
                {
                    //string pathAvatar = $"Resource/Avatar/{Guid.NewGuid().ToString("N")}";
                    //string pathFile = Path.Combine(webHostEnvironment.ContentRootPath, pathAvatar);
                    //DataHelpers.Base64ToImage(group.Avatar.Replace("data:image/png;base64,", ""), pathFile);

                    var fileName = Guid.NewGuid().ToString() + ".jpg";
                    var data = group.Avatar.Replace("data:image/png;base64,", "");
                    byte[] imageBytes = Convert.FromBase64String(data);
                    string container = "blobcontainer";
                    string blobconnect = "DefaultEndpointsProtocol=https;AccountName=pnchatstorage;AccountKey=kxdoY/j/U+Bg3MGLMzFavw40hz575PPP3sEFYzCuOJxjHrCUf9an0Of0WyOwfNNk1y+51U0HTGAG+AStitfbbQ==;EndpointSuffix=core.windows.net";

                    //var blobClient = new BlobClient(blobconnect, container, fileName);

                    //using (var stream = new MemoryStream(imageBytes))
                    //{
                    //    await blobClient.UploadAsync(stream);
                    //}
                    //var urlImg = blobClient.Uri.AbsoluteUri;

                    //grp.Avatar = group.Avatar = urlImg;
                }
                await chatContext.SaveChangesAsync();
            }
            return group;
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
                string path = Path.Combine(webHostEnvironment.ContentRootPath, $"Resource/Attachment/{DateTime.Now.Year}");
                FileHelper.CreateDirectory(path);
                try
                {
                    if (message.Attachments[0].Length > 0)
                    {
                        string pathFile = path + "/" + message.Attachments[0].FileName;
                        if (!File.Exists(pathFile))
                        {
                            using (var stream = new MemoryStream())
                            {
                                await message.Attachments[0].CopyToAsync(stream);
                                //await UploadBlobFile(message.Attachments[0]);
                            }
                        }
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
