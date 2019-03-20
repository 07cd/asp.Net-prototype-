USE [prototype]
GO

/****** Object:  Table [dbo].[user_question]    Script Date: 20/03/2019 14:07:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[user_question](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NULL,
	[question_id] [int] NULL,
 CONSTRAINT [PK__user_que__3213E83F1AF4761C] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[user_question] ADD  CONSTRAINT [DF__user_ques__user___32E0915F]  DEFAULT (NULL) FOR [user_id]
GO

ALTER TABLE [dbo].[user_question] ADD  CONSTRAINT [DF__user_ques__quest__33D4B598]  DEFAULT (NULL) FOR [question_id]
GO

ALTER TABLE [dbo].[user_question]  WITH CHECK ADD  CONSTRAINT [FK_user_question_question] FOREIGN KEY([question_id])
REFERENCES [dbo].[question] ([id])
ON UPDATE CASCADE
ON DELETE SET NULL
GO

ALTER TABLE [dbo].[user_question] CHECK CONSTRAINT [FK_user_question_question]
GO

ALTER TABLE [dbo].[user_question]  WITH CHECK ADD  CONSTRAINT [FK_user_question_user] FOREIGN KEY([user_id])
REFERENCES [dbo].[user] ([id])
GO

ALTER TABLE [dbo].[user_question] CHECK CONSTRAINT [FK_user_question_user]
GO

