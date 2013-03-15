CREATE TABLE [dbo].[PhunContent](
	[id] [int] NOT NULL,
	[parentdirectoryid] [int] NULL,
	[phuncontentdataid] [uniqueidentifier] NULL,
	[name] [nvarchar](100) NOT NULL,
	[path] [nvarchar](250) NOT NULL,
	[hostname] [nvarchar](200) NOT NULL,
	[datalength] [bigint] NOT NULL,
	[modifydate] [datetime] NULL,
	[modifyby] [nvarchar](50) NULL,
	[createdate] [datetime] NULL,
	[createby] [nvarchar](50) NULL,
 CONSTRAINT [PK_PhunContent] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[PhunContentData](
	[id] [uniqueidentifier] NOT NULL,
	[hostname] [nvarchar](200) NULL,
	[path] [nvarchar](250) NULL,
	[data] [image] NULL,
	[createdate] [datetime] NULL,
	[createby] [nvarchar](50) NULL,
 CONSTRAINT [PK_PhunContentData] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
