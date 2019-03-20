USE [prototype]
GO

/****** Object:  Table [dbo].[answer]    Script Date: 20/03/2019 14:06:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[answer](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[answer] [varchar](1500) NULL,
 CONSTRAINT [PK__answer__3213E83FA72B7A03] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[answer] ADD  CONSTRAINT [DF__answer__answer__22AA2996]  DEFAULT (NULL) FOR [answer]
GO

