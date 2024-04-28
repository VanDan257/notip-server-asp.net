CREATE TABLE [Group] (
  [Code] varchar(32) NOT NULL,
  [Type] varchar(32) NOT NULL,
  [Avatar] varchar(max),
  [Name] nvarchar(250),
  [Created] datetime NOT NULL,
  [CreatedBy] varchar(32) NOT NULL,
  [LastActive] datetime NOT NULL,
  PRIMARY KEY ([Code])
)
GO

CREATE TABLE [GroupCall] (
  [Code] varchar(32) NOT NULL,
  [Type] varchar(32) NOT NULL,
  [Avatar] varchar(max),
  [Name] nvarchar(250),
  [Created] datetime NOT NULL,
  [CreatedBy] varchar(32) NOT NULL,
  [LastActive] datetime NOT NULL,
  PRIMARY KEY ([Code])
)
GO

CREATE TABLE [User] (
  [Code] varchar(32) NOT NULL,
  [UserName] varchar(32),
  [Password] varchar(124),
  [FullName] nvarchar(50),
  [Dob] varchar(50),
  [Phone] varchar(50),
  [Email] varchar(50),
  [Address] nvarchar(255),
  [Avatar] varchar(max),
  [Gender] nvarchar(10),
  [LastLogin] datetime,
  [CurrentSession] varchar(500),
  PRIMARY KEY ([Code])
)
GO

CREATE TABLE [Call] (
  [Id] int NOT NULL IDENTITY(1, 1),
  [GroupCallCode] varchar(32) NOT NULL,
  [UserCode] varchar(32) NOT NULL,
  [Url] nvarchar(500) NOT NULL,
  [Status] varchar(32) NOT NULL,
  [Created] datetime NOT NULL,
  PRIMARY KEY ([Id])
)
GO

CREATE TABLE [Contact] (
  [Id] bigint NOT NULL IDENTITY(1, 1),
  [UserCode] varchar(32) NOT NULL,
  [ContactCode] varchar(32) NOT NULL,
  [Created] datetime NOT NULL,
  PRIMARY KEY ([Id])
)
GO

CREATE TABLE [GroupUser] (
  [Id] bigint NOT NULL IDENTITY(1, 1),
  [GroupCode] varchar(32) NOT NULL,
  [UserCode] varchar(32) NOT NULL,
  PRIMARY KEY ([Id])
)
GO

CREATE TABLE [Message] (
  [Id] bigint NOT NULL IDENTITY(1, 1),
  [Type] varchar(10) NOT NULL,
  [GroupCode] varchar(32) NOT NULL,
  [Content] nvarchar(max),
  [Path] nvarchar(255),
  [Created] datetime NOT NULL,
  [CreatedBy] varchar(32) NOT NULL,
  PRIMARY KEY ([Id])
)
GO

CREATE INDEX [IX_Call_GroupCallCode] ON [Call] ("GroupCallCode")
GO

CREATE INDEX [IX_Call_UserCode] ON [Call] ("UserCode")
GO

CREATE INDEX [IX_Contact_ContactCode] ON [Contact] ("ContactCode")
GO

CREATE INDEX [IX_Contact_UserCode] ON [Contact] ("UserCode")
GO

CREATE INDEX [IX_GroupUser_GroupCode] ON [GroupUser] ("GroupCode")
GO

CREATE INDEX [IX_GroupUser_UserCode] ON [GroupUser] ("UserCode")
GO

CREATE INDEX [IX_Message_CreatedBy] ON [Message] ("CreatedBy")
GO

CREATE INDEX [IX_Message_GroupCode] ON [Message] ("GroupCode")
GO

ALTER TABLE [Call] ADD CONSTRAINT [FK_Call_GroupCall] FOREIGN KEY ([GroupCallCode]) REFERENCES [GroupCall] ([Code])
GO

ALTER TABLE [Call] ADD CONSTRAINT [FK_Call_User] FOREIGN KEY ([UserCode]) REFERENCES [User] ([Code])
GO

ALTER TABLE [Contact] ADD CONSTRAINT [FK_Contact_User] FOREIGN KEY ([UserCode]) REFERENCES [User] ([Code])
GO

ALTER TABLE [Contact] ADD CONSTRAINT [FK_Contact_User1] FOREIGN KEY ([ContactCode]) REFERENCES [User] ([Code])
GO

ALTER TABLE [GroupUser] ADD CONSTRAINT [FK_GroupUser_Group] FOREIGN KEY ([GroupCode]) REFERENCES [Group] ([Code])
GO

ALTER TABLE [GroupUser] ADD CONSTRAINT [FK_GroupUser_User] FOREIGN KEY ([UserCode]) REFERENCES [User] ([Code])
GO

ALTER TABLE [Message] ADD CONSTRAINT [FK_Message_Group] FOREIGN KEY ([GroupCode]) REFERENCES [Group] ([Code])
GO

ALTER TABLE [Message] ADD CONSTRAINT [FK_Message_User] FOREIGN KEY ([CreatedBy]) REFERENCES [User] ([Code])
GO
