alter user [af-user] with password = '<your-own-password>'
go

grant connect to [af-user]
go

grant execute on schema::[web] to [af-user]
go
