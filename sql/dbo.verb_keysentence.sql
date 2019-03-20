USE [prototype]
GO

/****** Object:  Table [dbo].[verb_keysentence]    Script Date: 20/03/2019 14:08:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[verb_keysentence](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[verb_id] [int] NOT NULL,
	[keysentence_id] [int] NULL,
 CONSTRAINT [PK_verb_keysentence] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[verb_keysentence]  WITH CHECK ADD  CONSTRAINT [FK_verb_keysentence_keysentence] FOREIGN KEY([keysentence_id])
REFERENCES [dbo].[keysentence] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[verb_keysentence] CHECK CONSTRAINT [FK_verb_keysentence_keysentence]
GO

ALTER TABLE [dbo].[verb_keysentence]  WITH CHECK ADD  CONSTRAINT [FK_verb_keysentence_verb] FOREIGN KEY([verb_id])
REFERENCES [dbo].[verb] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[verb_keysentence] CHECK CONSTRAINT [FK_verb_keysentence_verb]
GO

