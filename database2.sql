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

create table imported_file_identifiers (
	id uniqueidentifier primary key not null	
)
go

alter table assignment alter column content_comment nvarchar(4000)
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
	@content_comment nvarchar(4000),
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
	@content_comment nvarchar(4000),
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

if OBJECT_ID('dbo.accreditation_term_x_analysis_method', 'U') is not null drop table accreditation_term_x_analysis_method;
if OBJECT_ID('dbo.accreditation_term_x_laboratory', 'U') is not null drop table accreditation_term_x_laboratory;
if OBJECT_ID('dbo.accreditation_term_x_nuclide', 'U') is not null drop table accreditation_term_x_nuclide;
if OBJECT_ID('dbo.accreditation_term_x_preparation_method', 'U') is not null drop table accreditation_term_x_preparation_method;
if OBJECT_ID('dbo.accreditation_term_x_sample_type', 'U') is not null drop table accreditation_term_x_sample_type;

if OBJECT_ID('dbo.accreditation_term', 'U') is not null drop table accreditation_term;

create table accreditation_term (
	id uniqueidentifier primary key not null,	
	name nvarchar(256) unique not null,
	fill_height_min float default null,
	fill_height_max float default null,
	weight_min float default null,
	weight_max float default null,
	volume_min float default null,
	volume_max float default null,	
	density_min float default null,
	density_max float default null,
	instance_status_id int not null default 1,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_accreditation_term
	@id uniqueidentifier,
	@name nvarchar(256),
	@fill_height_min float,
	@fill_height_max float,
	@weight_min float,
	@weight_max float,
	@volume_min float,
	@volume_max float,	
	@density_min float,
	@density_max float,
	@instance_status_id int,
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as 
	insert into accreditation_term values (
		@id,
		@name,
		@fill_height_min,
		@fill_height_max,
		@weight_min,
		@weight_max,
		@volume_min,
		@volume_max,		
		@density_min,
		@density_max,
		@instance_status_id,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_accreditation_term
	@id uniqueidentifier,
	@name nvarchar(256),
	@fill_height_min float,
	@fill_height_max float,
	@weight_min float,
	@weight_max float,
	@volume_min float,
	@volume_max float,	
	@density_min float,
	@density_max float,
	@instance_status_id int,	
	@update_date datetime,
	@update_id uniqueidentifier
as 
	update accreditation_term set 		
		name = @name,
		fill_height_min = @fill_height_min,
		fill_height_max = @fill_height_max,
		weight_min = @weight_min,
		weight_max = @weight_max,
		volume_min = @volume_min,
		volume_max = @volume_max,		
		density_min = @density_min,
		density_max = @density_max,
		instance_status_id = @instance_status_id,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_accreditation_term
	@id uniqueidentifier
as
	select * from accreditation_term where id = @id
go

create proc csp_select_accreditation_terms
	@instance_status_level int
as
	select acc.* from accreditation_term acc, instance_status st
	where acc.instance_status_id = st.id and acc.instance_status_id <= @instance_status_level
	order by create_date
go

create proc csp_select_accreditation_terms_flat
	@instance_status_level int
as
	select 
		acc.id,
		acc.name,
		acc.fill_height_min, 
		acc.fill_height_max, 
		acc.weight_min,
		acc.weight_max,
		acc.volume_min,
		acc.volume_max,
		acc.density_min,
		acc.density_max,
		st.name as 'instance_status_name',
		acc.create_date,
		acc.create_id,
		acc.update_date,
		acc.update_id
	from accreditation_term acc
		inner join instance_status st on acc.instance_status_id = st.id and acc.instance_status_id <= @instance_status_level
	order by acc.name
go

alter table analysis_result alter column activity_uncertainty_abs float null
go

/* tbl accreditation_term_x_laboratory */

if OBJECT_ID('dbo.accreditation_term_x_laboratory', 'U') is not null drop table accreditation_term_x_laboratory;

create table accreditation_term_x_laboratory (
	accreditation_term_id uniqueidentifier not null,
	laboratory_id uniqueidentifier not null
)
go

/* tbl accreditation_term_x_sample_type */

if OBJECT_ID('dbo.accreditation_term_x_sample_type', 'U') is not null drop table accreditation_term_x_sample_type;

create table accreditation_term_x_sample_type (
	accreditation_term_id uniqueidentifier not null,
	sample_type_id uniqueidentifier not null
)
go

/* tbl accreditation_term_x_preparation_method */

if OBJECT_ID('dbo.accreditation_term_x_sample_type_x_sample_component', 'U') is not null drop table accreditation_term_x_sample_type_x_sample_component;

create table accreditation_term_x_sample_type_x_sample_component (
	accreditation_term_id uniqueidentifier not null,
	sample_type_id uniqueidentifier not null,
	sample_component_id uniqueidentifier not null
)
go

/* tbl accreditation_term_x_preparation_method */

if OBJECT_ID('dbo.accreditation_term_x_preparation_method', 'U') is not null drop table accreditation_term_x_preparation_method;

create table accreditation_term_x_preparation_method (
	accreditation_term_id uniqueidentifier not null,
	preparation_method_id uniqueidentifier not null
)
go

/* tbl accreditation_term_x_analysis_method */

if OBJECT_ID('dbo.accreditation_term_x_analysis_method', 'U') is not null drop table accreditation_term_x_analysis_method;

create table accreditation_term_x_analysis_method (
	accreditation_term_id uniqueidentifier not null,
	analysis_method_id uniqueidentifier not null
)
go

/* tbl accreditation_term_x_nuclide */

if OBJECT_ID('dbo.accreditation_term_x_nuclide', 'U') is not null drop table accreditation_term_x_nuclide;

create table accreditation_term_x_nuclide (
	accreditation_term_id uniqueidentifier not null,
	nuclide_id uniqueidentifier not null
)
go

alter table accreditation_term_x_laboratory add foreign key (accreditation_term_id) references accreditation_term(id);
alter table accreditation_term_x_laboratory add foreign key (laboratory_id) references laboratory(id);

alter table accreditation_term_x_sample_type add foreign key (accreditation_term_id) references accreditation_term(id);
alter table accreditation_term_x_sample_type add foreign key (sample_type_id) references sample_type(id);

alter table accreditation_term_x_preparation_method add foreign key (accreditation_term_id) references accreditation_term(id);
alter table accreditation_term_x_preparation_method add foreign key (preparation_method_id) references preparation_method(id);

alter table accreditation_term_x_analysis_method add foreign key (accreditation_term_id) references accreditation_term(id);
alter table accreditation_term_x_analysis_method add foreign key (analysis_method_id) references analysis_method(id);

alter table accreditation_term_x_nuclide add foreign key (accreditation_term_id) references accreditation_term(id);
alter table accreditation_term_x_nuclide add foreign key (nuclide_id) references nuclide(id);
go

alter table preparation_geometry add volume_l float default null, radius_mm float default null
go

alter table preparation add volume_l float default null, preprocessing_volume_l float default null
go

alter proc csp_insert_preparation
	@id uniqueidentifier,
	@sample_id uniqueidentifier,
	@number int,
	@assignment_id uniqueidentifier,
	@laboratory_id uniqueidentifier,
	@preparation_geometry_id uniqueidentifier,
	@preparation_method_id uniqueidentifier,
	@workflow_status_id int,
	@amount float,
	@prep_unit_id int,
	@quantity float,
	@quantity_unit_id int,
	@fill_height_mm float,
	@instance_status_id int,
	@comment nvarchar(1000),	
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier,
	@volume_l float,
	@preprocessing_volume_l float
as 
	insert into preparation values (
		@id,
		@sample_id,
		@number,
		@assignment_id,
		@laboratory_id,
		@preparation_geometry_id,
		@preparation_method_id,
		@workflow_status_id,
		@amount,
		@prep_unit_id,
		@quantity,
		@quantity_unit_id,
		@fill_height_mm,
		@instance_status_id,
		@comment,	
		@create_date,
		@create_id,
		@update_date,
		@update_id,
		@volume_l,
		@preprocessing_volume_l
	);
go

alter proc csp_update_preparation
	@id uniqueidentifier,
	@preparation_geometry_id uniqueidentifier,
	@workflow_status_id int,
	@amount float,
	@prep_unit_id int,
	@quantity float,
	@quantity_unit_id int,
	@fill_height_mm float,	
	@comment nvarchar(1000),
	@update_date datetime,
	@update_id uniqueidentifier,
	@volume_l float,
	@preprocessing_volume_l float
as 
	update preparation set								
		preparation_geometry_id = @preparation_geometry_id,		
		workflow_status_id = @workflow_status_id,
		amount = @amount,
		prep_unit_id = @prep_unit_id,
		quantity = @quantity,
		quantity_unit_id = @quantity_unit_id,
		fill_height_mm = @fill_height_mm,	
		comment = @comment,			
		update_date = @update_date,
		update_id = @update_id,
		volume_l = @volume_l,
		preprocessing_volume_l = @preprocessing_volume_l
	where id = @id
go

alter proc csp_select_preparation_flat
	@id uniqueidentifier
as
	select 
		p.id,
		p.number as 'preparation_number',
		s.number as 'sample_number',
		ass.name as 'assignment_name',
		l.name as 'laboratory_name',
		pg.name as 'preparation_geometry_name',
		pm.name as 'preparation_method_name',
		ws.name as 'workflow_status_name',
		p.amount,
		pu.name as 'amount_unit_name',
		p.quantity,
		qu.name as 'quantity_unit_name',
		p.fill_height_mm,
		p.volume_l,
		p.preprocessing_volume_l,
		inst.name as 'instance_status_name',
		p.comment,
		p.create_date,
		p.create_id,
		p.update_date,
		p.update_id
	from preparation p
		left outer join sample s on s.id = p.sample_id
		left outer join assignment ass on ass.id = p.assignment_id
		left outer join laboratory l on l.id = p.laboratory_id
		left outer join preparation_geometry pg on pg.id = p.preparation_geometry_id
		left outer join preparation_method pm on pm.id = p.preparation_method_id
		left outer join workflow_status ws on ws.id = p.workflow_status_id
		left outer join preparation_unit pu on pu.id = p.prep_unit_id
		left outer join quantity_unit qu on qu.id = p.quantity_unit_id
		left outer join instance_status inst on inst.id = p.instance_status_id
	where p.id = @id
go

alter proc csp_insert_preparation_geometry
	@id uniqueidentifier,
	@name nvarchar(80),
	@min_fill_height float,
	@max_fill_height float,
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier,
	@volume_l float,
	@radius_mm float
as 
	insert into preparation_geometry values (
		@id,
		@name,
		@min_fill_height,
		@max_fill_height,
		@instance_status_id,
		@comment,		
		@create_date,
		@create_id,
		@update_date,
		@update_id,
		@volume_l,
		@radius_mm
	);
go

alter proc csp_update_preparation_geometry
	@id uniqueidentifier,
	@name nvarchar(80),
	@min_fill_height float,
	@max_fill_height float,
	@instance_status_id int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@update_id uniqueidentifier,
	@volume_l float,
	@radius_mm float
as 
	update preparation_geometry set 
		name = @name,
		min_fill_height_mm = @min_fill_height,
		max_fill_height_mm = @max_fill_height,
		instance_status_id = @instance_status_id,
		comment = @comment,		
		update_date = @update_date,
		update_id = @update_id,
		volume_l = @volume_l,
		radius_mm = @radius_mm
	where id = @id
go

alter proc csp_select_preparation_geometries_flat
	@instance_status_level int
as
	select 
		pb.id,
		pb.name,
		pb.min_fill_height_mm, 
		pb.max_fill_height_mm, 
		pb.volume_l,
		pb.radius_mm,
		st.name as 'instance_status_name',		
		pb.comment,
		pb.create_date,
		pb.create_id,
		pb.update_date,
		pb.update_id		
	from preparation_geometry pb, instance_status st
	where pb.instance_status_id = st.id and pb.instance_status_id <= @instance_status_level
	order by pb.name
go

update counters set value = 2 where name = 'database_version'
go