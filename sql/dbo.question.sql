USE [prototype]
GO

/****** Object:  Table [dbo].[question]    Script Date: 20/03/2019 14:07:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[question](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[question] [varchar](1500) NULL,
 CONSTRAINT [PK__question__3213E83F17AE69DA] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[question] ADD  CONSTRAINT [DF__question__questi__2F10007B]  DEFAULT (NULL) FOR [question]
GO

