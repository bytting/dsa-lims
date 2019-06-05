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

alter table assignment add description nvarchar(80) default null
go

alter proc csp_insert_assignment
	@id uniqueidentifier,
	@name nvarchar(80),	
	@laboratory_id uniqueidentifier,
	@account_id uniqueidentifier,
	@deadline datetime,
	@requested_sigma_act float,	
	@requested_sigma_mda float,	
	@customer_company_name nvarchar(80),	
	@customer_company_email nvarchar(80),
	@customer_company_phone nvarchar(80),
	@customer_company_address nvarchar(256),
	@customer_contact_name nvarchar(80),	
	@customer_contact_email nvarchar(80),
	@customer_contact_phone nvarchar(80),
	@customer_contact_address nvarchar(256),
	@approved_customer bit,
	@approved_customer_by nvarchar(50),
	@approved_laboratory bit,	
	@approved_laboratory_by nvarchar(50),
	@content_comment nvarchar(1000),
	@report_comment nvarchar(1000),	
	@audit_comment nvarchar(4000),	
	@workflow_status_id int,
	@last_workflow_status_date datetime,
	@last_workflow_status_by nvarchar(50),
	@analysis_report_version int,
	@instance_status_id int,
	@locked_id uniqueidentifier,
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier,
	@description nvarchar(80)
as 		
	insert into assignment values (
		@id,
		@name,
		@laboratory_id,
		@account_id,
		@deadline,
		@requested_sigma_act,
		@requested_sigma_mda,
		@customer_company_name,		
		@customer_company_email,
		@customer_company_phone,
		@customer_company_address,
		@customer_contact_name,		
		@customer_contact_email,
		@customer_contact_phone,
		@customer_contact_address,
		@approved_customer,
		@approved_customer_by,
		@approved_laboratory,	
		@approved_laboratory_by,
		@content_comment,
		@report_comment,
		@audit_comment,
		@workflow_status_id,
		@last_workflow_status_date,
		@last_workflow_status_by,
		@analysis_report_version,
		@instance_status_id,
		@locked_id,
		@create_date,
		@create_id,
		@update_date,
		@update_id,
		@description
	);
go

alter proc csp_update_assignment
	@id uniqueidentifier,
	@name nvarchar(80),	
	@laboratory_id uniqueidentifier,
	@account_id uniqueidentifier,
	@deadline datetime,
	@requested_sigma_act float,	
	@requested_sigma_mda float,	
	@customer_company_name nvarchar(80),	
	@customer_company_email nvarchar(80),
	@customer_company_phone nvarchar(80),
	@customer_company_address nvarchar(256),
	@customer_contact_name nvarchar(80),	
	@customer_contact_email nvarchar(80),
	@customer_contact_phone nvarchar(80),
	@customer_contact_address nvarchar(256),
	@approved_customer bit,
	@approved_customer_by nvarchar(50),
	@approved_laboratory bit,	
	@approved_laboratory_by nvarchar(50),
	@content_comment nvarchar(1000),
	@report_comment nvarchar(1000),	
	@audit_comment nvarchar(4000),	
	@workflow_status_id int,
	@last_workflow_status_date datetime,
	@last_workflow_status_by nvarchar(50),
	@analysis_report_version int,
	@instance_status_id int,	
	@locked_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier,
	@description nvarchar(80)
as 		
	update assignment set		
		name = @name,
		laboratory_id = @laboratory_id,
		account_id = @account_id,
		deadline = @deadline,
		requested_sigma_act = @requested_sigma_act,
		requested_sigma_mda = @requested_sigma_mda,
		customer_company_name = @customer_company_name,		
		customer_company_email = @customer_company_email,
		customer_company_phone = @customer_company_phone,
		customer_company_address = @customer_company_address,
		customer_contact_name = @customer_contact_name,
		customer_contact_email = @customer_contact_email,
		customer_contact_phone = @customer_contact_phone,
		customer_contact_address = @customer_contact_address,
		approved_customer = @approved_customer,
		approved_customer_by = @approved_customer_by,
		approved_laboratory = @approved_laboratory,	
		approved_laboratory_by = @approved_laboratory_by,
		content_comment = @content_comment,
		report_comment = @report_comment,
		audit_comment = @audit_comment,
		workflow_status_id = @workflow_status_id,
		last_workflow_status_date = @last_workflow_status_date,
		last_workflow_status_by = @last_workflow_status_by,
		analysis_report_version = @analysis_report_version,
		instance_status_id = @instance_status_id,				
		locked_id = @locked_id,
		update_date = @update_date,
		update_id = @update_id,
		description = @description
	where id = @id
go

alter table sample add lod_weight_ash float default null, lod_temperature_ash float default null, lod_water_percent float default null, lod_water_percent_ash float default null, lod_factor float default null, lod_factor_ash float default null
go

alter table sample add lod_weight_end_ash float default null
go

alter table sample add lod_weight_ash2 float default null, lod_weight_end_ash2 float default null, lod_temperature_ash2 float default null, lod_water_percent_ash2 float default null, lod_factor_ash2 float default null
go

alter proc csp_update_sample_info
	@id uniqueidentifier,	
	@wet_weight_g float,	
	@dry_weight_g float,
	@volume_l float,
	@lod_weight_start float,	
	@lod_weight_end float,		
	@lod_temperature float,
	@lod_water_percent float,
	@lod_factor float,
	@lod_weight_ash float,
	@lod_weight_end_ash float,
	@lod_temperature_ash float,	
	@lod_water_percent_ash float,	
	@lod_factor_ash float,
	@lod_weight_ash2 float,
	@lod_weight_end_ash2 float,
	@lod_temperature_ash2 float,	
	@lod_water_percent_ash2 float,	
	@lod_factor_ash2 float,
	@update_date datetime,
	@update_id uniqueidentifier	
as 		
	update sample set		
		wet_weight_g = @wet_weight_g,	
		dry_weight_g = @dry_weight_g,
		volume_l = @volume_l,
		lod_weight_start = @lod_weight_start,	
		lod_weight_end = @lod_weight_end,	
		lod_temperature = @lod_temperature,
		lod_water_percent = @lod_water_percent,
		lod_factor = @lod_factor,
		lod_weight_ash = @lod_weight_ash,			
		lod_weight_end_ash = @lod_weight_end_ash,			
		lod_temperature_ash = @lod_temperature_ash,		
		lod_water_percent_ash = @lod_water_percent_ash,		
		lod_factor_ash = @lod_factor_ash,
		lod_weight_ash2 = @lod_weight_ash2,			
		lod_weight_end_ash2 = @lod_weight_end_ash2,			
		lod_temperature_ash2 = @lod_temperature_ash2,		
		lod_water_percent_ash2 = @lod_water_percent_ash2,		
		lod_factor_ash2 = @lod_factor_ash2,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

alter proc csp_insert_sample
	@id uniqueidentifier,	
	@number int,
	@laboratory_id uniqueidentifier,	
	@sample_type_id uniqueidentifier,	
	@sample_storage_id uniqueidentifier,
	@sample_component_id uniqueidentifier,
	@project_sub_id uniqueidentifier,
	@station_id uniqueidentifier,
	@sampler_id uniqueidentifier,
	@sampling_method_id uniqueidentifier,
	@transform_from_id uniqueidentifier,	
	@transform_to_id uniqueidentifier,		
	@imported_from nvarchar(128),
	@imported_from_id nvarchar(128),		
	@municipality_id uniqueidentifier,
	@location_type nvarchar(50),
	@location nvarchar(128),	
	@latitude float,
	@longitude float,
	@altitude float,
	@sampling_date_from datetime,
	@sampling_date_to datetime,
	@reference_date datetime,
	@external_id nvarchar(128),
	@wet_weight_g float,	
	@dry_weight_g float,
	@volume_l float,
	@lod_weight_start float,	
	@lod_weight_end float,	
	@lod_temperature float,
	@lod_water_percent float,
	@lod_factor float,
	@lod_weight_ash float,		
	@lod_weight_end_ash float,		
	@lod_temperature_ash float,	
	@lod_water_percent_ash float,	
	@lod_factor_ash float,
	@lod_weight_ash2 float,		
	@lod_weight_end_ash2 float,		
	@lod_temperature_ash2 float,	
	@lod_water_percent_ash2 float,	
	@lod_factor_ash2 float,
	@confidential bit,		
	@instance_status_id int,
	@locked_id uniqueidentifier,
	@comment nvarchar(1000),	
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier	
as
	insert into sample values (
		@id,
		@number,
		@laboratory_id,	
		@sample_type_id,	
		@sample_storage_id,
		@sample_component_id,
		@project_sub_id,
		@station_id,
		@sampler_id,
		@sampling_method_id,
		@transform_from_id,	
		@transform_to_id,			
		@imported_from,
		@imported_from_id,		
		@municipality_id,
		@location_type,
		@location,	
		@latitude,
		@longitude,
		@altitude,
		@sampling_date_from,
		@sampling_date_to,
		@reference_date,
		@external_id,
		@wet_weight_g,	
		@dry_weight_g,
		@volume_l,
		@lod_weight_start,	
		@lod_weight_end,			
		@lod_temperature,				
		@confidential,		
		@instance_status_id,
		@locked_id,
		@comment,	
		@create_date,
		@create_id,
		@update_date,
		@update_id,
		@lod_weight_ash,	
		@lod_temperature_ash,
		@lod_water_percent,
		@lod_water_percent_ash,
		@lod_factor,
		@lod_factor_ash,
		@lod_weight_end_ash,
		@lod_weight_ash2,
		@lod_weight_end_ash2,
		@lod_temperature_ash2,
		@lod_water_percent_ash2,
		@lod_factor_ash2
	);
go

update counters set value = 2 where name = 'database_version'
go