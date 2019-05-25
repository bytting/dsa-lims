use dsa_lims
go

alter proc csp_select_assignments_short
	@instance_status_level int
as
	select id, name
	from assignment
	where instance_status_id <= @instance_status_level
	order by create_date desc
go

if OBJECT_ID('dbo.audit_log', 'U') is not null drop table audit_log;

create table audit_log (
	id uniqueidentifier primary key not null,
	source_table nvarchar(50) not null,
	source_id uniqueidentifier not null,	
	operation nvarchar(50) not null,
	comment nvarchar(200) default null,
	value varbinary(max) not null,
	create_date datetime not null
)
go

create proc csp_insert_audit_log
	@id uniqueidentifier,
	@source_table nvarchar(50),
	@source_id uniqueidentifier,
	@operation nvarchar(50),
	@comment nvarchar(200),
	@value varbinary(max),
	@create_date datetime
as
	insert into audit_log values(@id, @source_table, @source_id, @operation, @comment, @value, @create_date);
go

create proc csp_select_audit_log
	@id uniqueidentifier
as 
	select * from audit_log where id = @id
go

create proc csp_select_audit_logs
	@source_table nvarchar(50),
	@source_id uniqueidentifier
as 
	select * from audit_log where source_table = @source_table and source_id = @source_id
go

update counters set value = 2 where name = 'database_version'