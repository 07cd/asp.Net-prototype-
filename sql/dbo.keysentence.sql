USE [prototype]
GO

/****** Object:  Table [dbo].[keysentence]    Script Date: 20/03/2019 14:07:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[keysentence](
	[question_id] [int] NULL,
	[answer_id] [int] NULL,
	[id] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK__keysente__3213E83F078CB79B] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[keysentence]  WITH CHECK ADD  CONSTRAINT [FK_keysentence_answer1] FOREIGN KEY([answer_id])
REFERENCES [dbo].[answer] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[keysentence] CHECK CONSTRAINT [FK_keysentence_answer1]
GO

ALTER TABLE [dbo].[keysentence]  WITH CHECK ADD  CONSTRAINT [FK_keysentence_question1] FOREIGN KEY([question_id])
REFERENCES [dbo].[question] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[keysentence] CHECK CONSTRAINT [FK_keysentence_question1]
GO

