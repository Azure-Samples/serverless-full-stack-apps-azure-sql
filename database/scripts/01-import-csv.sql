/*
	Full documentation on "Accessing data in a CSV file referencing an Azure blob storage location" here:
	https://docs.microsoft.com/en-us/sql/relational-databases/import-export/examples-of-bulk-access-to-data-in-azure-blob-storage?view=sql-server-ver15
*/
create master key encryption by password = '<insert strong password>'
go

create database scoped credential AzureBlobCredentials
with identity = 'SHARED ACCESS SIGNATURE',
secret = 'sp=r&st=2021-03-12T00:47:24Z&se=2025-03-11T07:47:24Z&spr=https&sv=2020-02-10&sr=c&sig=BmuxFevKhWgbvo%2Bj8TlLYObjbB7gbvWzQaAgvGcg50c%3D' -- Omit any leading question mark
go

create external data source RouteData
with (
	type = blob_storage,
	location = 'https://azuresqlworkshopsa.blob.core.windows.net/bus',
	credential = AzureBlobCredentials
)

delete from dbo.[Routes];
go

insert into dbo.[Routes]
	([Id], [AgencyId], [ShortName], [Description], [Type])
select 
	[Id], [AgencyId], [ShortName], [Description], [Type]
from 
openrowset
	( 
		bulk 'routes.txt', 
		data_source = 'RouteData', 
		formatfile = 'routes.fmt', 
		formatfile_data_source = 'RouteData', 
		firstrow=2,
		format='csv'
	) t;
go

select * from dbo.[Routes] where [Description] like '%Education Hill%'
go

select * from sys.[database_credentials]
select * from sys.[external_data_sources]
go