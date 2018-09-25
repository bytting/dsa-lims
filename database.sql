/*	
	DSA Lims - Laboratory Information Management System
    Copyright (C) 2018  Norwegian Radiation Protection Authority

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
	
	Authors: Dag Robole,
*/

create database dsa_lims
go

USE dsa_lims
go

/*===========================================================================*/
/* tbl instance_status */

if OBJECT_ID('dbo.instance_status', 'U') IS NOT NULL drop table instance_status;

create table instance_status (
	id int primary key NOT NULL,
	name nvarchar(20) NOT NULL	
)
go

insert into instance_status values(1, 'Active')
insert into instance_status values(2, 'Inactive')
insert into instance_status values(3, 'Deleted')
go

/*===========================================================================*/
/* tbl workflow_status */

if OBJECT_ID('dbo.workflow_status', 'U') IS NOT NULL drop table workflow_status;

create table workflow_status (
	id int primary key NOT NULL,
	name nvarchar(20) NOT NULL	
)
go

insert into workflow_status values(1, 'Construction')
insert into workflow_status values(2, 'Complete')
insert into workflow_status values(3, 'Rejected')
go

/*===========================================================================*/
/* tbl audit_log */

if OBJECT_ID('dbo.audit_log', 'U') IS NOT NULL drop table audit_log;

create table audit_log (
	id uniqueidentifier primary key NOT NULL,
	source_table nvarchar(50) NOT NULL,
	source_id uniqueidentifier NOT NULL,	
	operation nvarchar(50) NOT NULL,
	value nvarchar(max) NOT NULL,
	create_date datetime NOT NULL
)
go

create proc csp_insert_audit_message
	@id uniqueidentifier,
	@source_table nvarchar(50),
	@source_id uniqueidentifier,
	@operation nvarchar(50),
	@value nvarchar(max),
	@create_date datetime
as
	insert into audit_log values(@id, @source_table, @source_id, @operation, @value, @create_date);
go

/*===========================================================================*/
/* tbl counters */

if OBJECT_ID('dbo.counters', 'U') IS NOT NULL drop table counters;

create table counters (	
	name nvarchar(50) primary key NOT NULL,
	value int default 1	
)
go

insert into counters (name) values('sample_counter')
go

/*===========================================================================*/
/* tbl roles */

if OBJECT_ID('dbo.roles', 'U') IS NOT NULL drop table roles;

create table roles (	
	id int primary key NOT NULL,
	name nvarchar(64) unique NOT NULL
)
go

insert into roles (id, name) values(1, 'Administrator')
insert into roles (id, name) values(2, 'Laboratory Manager')
insert into roles (id, name) values(3, 'Laboratory Operator')
insert into roles (id, name) values(4, 'Order Manager')
insert into roles (id, name) values(5, 'Order Operator')
insert into roles (id, name) values(6, 'Sample Operator')
go

/*===========================================================================*/
/* tbl account */

if OBJECT_ID('dbo.account', 'U') IS NOT NULL drop table account;

create table account (	
	username nvarchar(50) primary key NOT NULL,	
	password_hash nchar(64) NOT NULL,
	fullname nvarchar(128) NOT NULL,
	laboratory_id uniqueidentifier default NULL,
	language_code nvarchar(8) default 'en',
	instance_status_id int default 1,
	create_date datetime NOT NULL,	
	update_date datetime NOT NULL
)
go

create proc csp_select_users
	@instance_status_level int
as 
	select * from account 
	where instance_status_id <= @instance_status_level
	order by username
go

create proc csp_select_users_short
as 
	select username, fullname from account order by username
go

create proc csp_select_users_flat
as 
	select 
		a.username,	
		a.password_hash,
		a.fullname,
		l.name as 'laboratory_name',
		a.language_code,
		st.name as 'instance_status',
		a.create_date,	
		a.update_date
	from account a left outer join laboratory l on a.laboratory_id = l.id inner join instance_status st on a.instance_status_id = st.id 
	order by username
go

/*===========================================================================*/
/* tbl preparation_unit */

if OBJECT_ID('dbo.preparation_unit', 'U') IS NOT NULL drop table preparation_unit;

create table preparation_unit (
	id int primary key NOT NULL,
	name nvarchar(20) NOT NULL	
)
go

insert into preparation_unit values(1, 'Wet weight (g)')
insert into preparation_unit values(2, 'Dry weight (g)')
insert into preparation_unit values(3, 'Volume (L)')
go

/*===========================================================================*/
/* tbl activity_unit */

if OBJECT_ID('dbo.activity_unit', 'U') IS NOT NULL drop table activity_unit;

create table activity_unit (
	id uniqueidentifier primary key NOT NULL,	
	name nvarchar(20) NOT NULL,
	convert_factor float default NULL,
	uniform_activity_unit_id int default NULL
)
go

insert into activity_unit values(NEWID(), 'Bq', 1.0, 1)
insert into activity_unit values(NEWID(), 'mBq/g', 1000.0, 2)
insert into activity_unit values(NEWID(), 'mBq/g ww', 1000.0, 2)
insert into activity_unit values(NEWID(), 'mBq/g dw', 1000.0, 2)
insert into activity_unit values(NEWID(), 'Bq/g', 1.0, 2)
insert into activity_unit values(NEWID(), 'Bq/g ww', 1.0, 2)
insert into activity_unit values(NEWID(), 'Bq/g dw', 1.0, 2)
insert into activity_unit values(NEWID(), 'Bq/kg', 0.001, 2)
insert into activity_unit values(NEWID(), 'Bq/kg ww', 0.001, 2)
insert into activity_unit values(NEWID(), 'Bq/kg dw', 0.001, 2)
insert into activity_unit values(NEWID(), 'Bq/m2', 1.0, 3)
insert into activity_unit values(NEWID(), 'Bq/m3', 1.0, 4)
insert into activity_unit values(NEWID(), 'mBq/l', 1.0, 4)
insert into activity_unit values(NEWID(), 'Bq/l', 1000.0, 4)
insert into activity_unit values(NEWID(), 'Bq/filter', 1.0, 1)
go

create proc csp_select_activity_units
as 
	select 
		id,	
		name,
		convert_factor,
		uniform_activity_unit_id
	from activity_unit
	order by name
go

create proc csp_select_activity_units_flat
as 
	select 
		au.id,	
		au.name,
		au.convert_factor,
		uau.name as 'uniform_activity_name'
	from activity_unit au, uniform_activity_unit uau where au.uniform_activity_unit_id = uau.id
	order by au.name
go

/*===========================================================================*/
/* tbl uniform_activity_unit */

if OBJECT_ID('dbo.uniform_activity_unit', 'U') IS NOT NULL drop table uniform_activity_unit;

create table uniform_activity_unit (
	id int primary key NOT NULL,
	name nvarchar(20) NOT NULL	
)
go

insert into uniform_activity_unit values(1, 'Bq')
insert into uniform_activity_unit values(2, 'Bq/g')
insert into uniform_activity_unit values(3, 'Bq/m2')
insert into uniform_activity_unit values(4, 'Bq/m3')

/*===========================================================================*/
/* tbl accreditation_term */

if OBJECT_ID('dbo.accreditation_term', 'U') IS NOT NULL drop table accreditation_term;

create table accreditation_term (
	id uniqueidentifier primary key NOT NULL,	
	density_min float default NULL,
	density_max float default NULL,
	dry_weight_min float default NULL,
	dry_weight_max float default NULL,
	wet_weight_min float default NULL,
	wet_weight_max float default NULL,
	volume_min float default NULL,
	volume_max float default NULL,
	fill_height_min float default NULL,
	fill_height_max float default NULL,
	instance_status_id int default 1,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl accreditation_term_x_laboratory */

if OBJECT_ID('dbo.accreditation_term_x_laboratory', 'U') IS NOT NULL drop table accreditation_term_x_laboratory;

create table accreditation_term_x_laboratory (
	accreditation_term_id uniqueidentifier NOT NULL,
	laboratory_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl accreditation_term_x_sample_type */

if OBJECT_ID('dbo.accreditation_term_x_sample_type', 'U') IS NOT NULL drop table accreditation_term_x_sample_type;

create table accreditation_term_x_sample_type (
	accreditation_term_id uniqueidentifier NOT NULL,
	sample_type_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl accreditation_term_x_preparation_method */

if OBJECT_ID('dbo.accreditation_term_x_preparation_method', 'U') IS NOT NULL drop table accreditation_term_x_preparation_method;

create table accreditation_term_x_preparation_method (
	accreditation_term_id uniqueidentifier NOT NULL,
	preparation_method_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl accreditation_term_x_analysis_method */

if OBJECT_ID('dbo.accreditation_term_x_analysis_method', 'U') IS NOT NULL drop table accreditation_term_x_analysis_method;

create table accreditation_term_x_analysis_method (
	accreditation_term_id uniqueidentifier NOT NULL,
	analysis_method_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl accreditation_term_x_nuclide */

if OBJECT_ID('dbo.accreditation_term_x_nuclide', 'U') IS NOT NULL drop table accreditation_term_x_nuclide;

create table accreditation_term_x_nuclide (
	accreditation_term_id uniqueidentifier NOT NULL,
	nuclide_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl county */

if OBJECT_ID('dbo.county', 'U') IS NOT NULL drop table county;

create table county (
	id uniqueidentifier primary key NOT NULL,	
	name nvarchar(128) unique NOT NULL,
	county_number int NOT NULL,	
	instance_status_id int default 1,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_select_county
	@id uniqueidentifier
as 
	select * from county where id = @id
go

create proc csp_insert_county
	@id uniqueidentifier,
	@name nvarchar(80),
	@county_number int,	
	@instance_status_id int,
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)	
as 
	insert into county values (
		@id,		
		@name,		
		@county_number,		
		@instance_status_id,
		@create_date,
		@created_by,
		@update_date,
		@updated_by
	);
go

create proc csp_update_county
	@id uniqueidentifier,
	@name nvarchar(80),
	@county_number int,	
	@instance_status_id int,
	@update_date datetime,
	@updated_by nvarchar(50)	
as 
	update county set 
		name = @name,
		county_number = @county_number,		
		instance_status_id = @instance_status_id,		
		update_date = @update_date,
		updated_by = @updated_by
	where id = @id
go

create proc csp_select_counties_flat
as
	select 
		id,
		name,
		county_number, 	
		instance_status_id,	
		create_date,
		created_by,
		update_date,
		updated_by
	from county
	order by name
go

/*===========================================================================*/
/* tbl municipality */

if OBJECT_ID('dbo.municipality', 'U') IS NOT NULL drop table municipality;

create table municipality (
	id uniqueidentifier primary key NOT NULL,
	county_id uniqueidentifier NOT NULL,
	name nvarchar(128) NOT NULL,
	municipality_number int NOT NULL,		
	instance_status_id int default 1,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_select_municipality
	@id uniqueidentifier
as 
	select * from municipality where id = @id
go

create proc csp_select_municipalities_for_county
	@county_id uniqueidentifier
as 
	select 
		id,	
		county_id,
		name,
		municipality_number,
		instance_status_id,
		create_date,
		created_by,
		update_date,
		updated_by
	from municipality
	where county_id = @county_id
	order by name
go

create proc csp_insert_municipality
	@id uniqueidentifier,
	@county_id uniqueidentifier,
	@name nvarchar(80),
	@municipality_number int,	
	@instance_status_id int,
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)	
as 
	insert into municipality values (
		@id,		
		@county_id,
		@name,		
		@municipality_number,		
		@instance_status_id,
		@create_date,
		@created_by,
		@update_date,
		@updated_by
	);
go

create proc csp_update_municipality
	@id uniqueidentifier,
	@name nvarchar(80),
	@municipality_number int,	
	@instance_status_id int,
	@update_date datetime,
	@updated_by nvarchar(50)	
as 
	update municipality set 
		name = @name,
		municipality_number = @municipality_number,		
		instance_status_id = @instance_status_id,		
		update_date = @update_date,
		updated_by = @updated_by
	where id = @id
go

/*===========================================================================*/
/* tbl customer */

if OBJECT_ID('dbo.customer', 'U') IS NOT NULL drop table customer;

create table customer (
	id uniqueidentifier primary key NOT NULL,
	name nvarchar(256) unique NOT NULL,
	address nvarchar(256) default NULL,
	email nvarchar(80) default NULL,
	phone nvarchar(80) default NULL,	
	instance_status_id int default 1,
	comment nvarchar(1000) default NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl customer_contact */

if OBJECT_ID('dbo.customer_contact', 'U') IS NOT NULL drop table customer_contact;

create table customer_contact (
	id uniqueidentifier primary key NOT NULL,
	customer_id uniqueidentifier NOT NULL,
	account_id uniqueidentifier default NULL,
	name nvarchar(256) NOT NULL,
	email nvarchar(80) default NULL,
	phone nvarchar(80) default NULL,	
	instance_status_id int default 1,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl sampler */

if OBJECT_ID('dbo.sampler', 'U') IS NOT NULL drop table sampler;

create table sampler (
	id uniqueidentifier primary key NOT NULL,
	name nvarchar(256) NOT NULL,
	address nvarchar(256) default NULL,
	email nvarchar(80) default NULL,
	phone nvarchar(80) default NULL,	
	instance_status_id int default 1,
	comment nvarchar(1000) default NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_select_samplers
as 
	select * from sampler order by name
go

create proc csp_select_samplers_flat
as 
	select 
		s.id,
		s.name,
		s.address,
		s.email,
		s.phone,	
		st.name as 'instance_status',
		s.comment,
		s.create_date,
		s.created_by,
		s.update_date,
		s.updated_by
	 from sampler s, instance_status st
	 where s.instance_status_id = st.id
	 order by s.name
go

create proc csp_select_sampler
	@id uniqueidentifier
as 
	select * from sampler where id = @id
go

create proc csp_insert_sampler
	@id uniqueidentifier,
	@name nvarchar(256),
	@address nvarchar(256),
	@email nvarchar(80),
	@phone nvarchar(80),
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)
as 
	insert into sampler values (
		@id,
		@name,
		@address,
		@email,
		@phone,
		@instance_status_id,
		@comment,
		@create_date,
		@created_by,
		@update_date,
		@updated_by
	);
go

create proc csp_update_sampler
	@id uniqueidentifier,
	@name nvarchar(256),
	@address nvarchar(256),
	@email nvarchar(80),
	@phone nvarchar(80),
	@instance_status_id int,
	@comment nvarchar(1000),
	@update_date datetime,
	@updated_by nvarchar(50)
as 
	update sampler set 
		name = @name,
		address = @address,
		email = @email,
		phone = @phone,
		instance_status_id = @instance_status_id,
		comment = @comment,	
		update_date = @update_date,
		updated_by = @updated_by
	where id = @id
go

/*===========================================================================*/
/* tbl laboratory */

if OBJECT_ID('dbo.laboratory', 'U') IS NOT NULL drop table laboratory;

create table laboratory (
	id uniqueidentifier primary key NOT NULL,
	name nvarchar(256) unique NOT NULL,
	name_prefix nvarchar(8) unique NOT NULL,
	address nvarchar(256) default NULL,
	email nvarchar(80) default NULL,
	phone nvarchar(80) default NULL,
	assignment_counter int default 1,
	comment nvarchar(1000) NOT NULL,	
	instance_status_id int default 1,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_select_laboratories
as
	select * from  laboratory order by name
go

create proc csp_select_laboratories_short
as
	select id, name	from laboratory order by name
go

create proc csp_select_laboratory
	@id uniqueidentifier	
as 
select * from laboratory where id = @id
go

create proc csp_insert_laboratory
	@id uniqueidentifier,
	@name nvarchar(256),
	@name_prefix nvarchar(8),
	@address nvarchar(256),
	@email nvarchar(80),
	@phone nvarchar(80),
	@assignment_counter int,		
	@comment nvarchar(1000),
	@instance_status_id int,
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)
as 
	insert into laboratory values (
		@id,
		@name,
		@name_prefix,
		@address,
		@email,
		@phone,
		@assignment_counter,
		@comment,
		@instance_status_id,
		@create_date,
		@created_by,
		@update_date,
		@updated_by
	);
go

create proc csp_update_laboratory
	@id uniqueidentifier,
	@name nvarchar(256),
	@name_prefix nvarchar(8),
	@address nvarchar(256),
	@email nvarchar(80),
	@phone nvarchar(80),
	@assignment_counter int,		
	@comment nvarchar(1000),
	@instance_status_id int,
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)
as 
	update laboratory set
		id = @id,
		name = @name,
		name_prefix = @name_prefix,
		address = @address,
		email = @email,
		phone = @phone,
		assignment_counter = @assignment_counter,
		comment = @comment,
		instance_status_id = @instance_status_id,
		create_date = @create_date,
		created_by = @created_by,
		update_date = @update_date,
		updated_by = @updated_by
	where id = @id
go

/*===========================================================================*/
/* tbl location_type */

if OBJECT_ID('dbo.location_type', 'U') IS NOT NULL drop table location_type;

create table location_type (
	id int primary key NOT NULL,
	name nvarchar(32) NOT NULL	
)
go

insert into location_type values(1, 'Organization number')
insert into location_type values(2, 'Business number')
insert into location_type values(3, 'Property unit number')
insert into location_type values(4, 'Place name')
insert into location_type values(5, 'Other')
go

create proc csp_select_location_types
as
	select id, name from location_type 
	order by name
go

/*===========================================================================*/
/* tbl assignment */

if OBJECT_ID('dbo.assignment', 'U') IS NOT NULL drop table assignment;

create table assignment (
	id uniqueidentifier primary key NOT NULL,
	name nvarchar(80) NOT NULL,	
	laboratory_id uniqueidentifier NOT NULL,
	workflow_status_id int default 1,
	customer_name nvarchar(256) default NULL,
	customer_address nvarchar(256) default NULL,
	customer_email nvarchar(80) default NULL,
	customer_phone nvarchar(80) default NULL,
	customer_contact_name nvarchar(256) default NULL,	
	customer_contact_email nvarchar(80) default NULL,
	customer_contact_phone nvarchar(80) default NULL,
	deadline datetime default NULL,
	approved_customer bit default 0,
	approved_laboratory bit default 0,
	comment nvarchar(1000) default NULL,
	content_comment nvarchar(1000) default NULL,
	report_comment nvarchar(1000) default NULL,		
	closed_date datetime default NULL,
	closed_by uniqueidentifier NOT NULL,
	instance_status_id int default 1,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl assignment_sample */

if OBJECT_ID('dbo.assignment_sample', 'U') IS NOT NULL drop table assignment_sample;

create table assignment_sample (
	id uniqueidentifier primary key NOT NULL,	
	assignment_id uniqueidentifier NOT NULL,	
	sample_type_id uniqueidentifier NOT NULL,	
	sample_count int NOT NULL,
	return_to_sender bit default 0,
	comment nvarchar(1000) default NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl assignment_preparation */

if OBJECT_ID('dbo.assignment_preparation', 'U') IS NOT NULL drop table assignment_preparation;

create table assignment_preparation (
	id uniqueidentifier primary key NOT NULL,	
	assignment_sample_id uniqueidentifier NOT NULL,		
	preparation_method_id uniqueidentifier NOT NULL,
	comment nvarchar(1000) default NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl assignment_analysis */

if OBJECT_ID('dbo.assignment_analysis', 'U') IS NOT NULL drop table assignment_analysis;

create table assignment_analysis (
	id uniqueidentifier primary key NOT NULL,	
	assignment_preparation_id uniqueidentifier default NULL,		
	analysis_method_id uniqueidentifier NOT NULL,
	comment nvarchar(1000) default NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl preparation_box */

if OBJECT_ID('dbo.preparation_box', 'U') IS NOT NULL drop table preparation_box;

create table preparation_box (
	id uniqueidentifier primary key NOT NULL,
	name nvarchar(80) NOT NULL,
	min_fill_height_mm float default 0,
	max_fill_height_mm float default 0,	
	instance_status_id int default 1,
	comment nvarchar(1000) default NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_select_geometry
	@id uniqueidentifier
as 
	select * from preparation_box where id = @id
go

create proc csp_insert_geometry
	@id uniqueidentifier,
	@name nvarchar(80),
	@min_fill_height float,
	@max_fill_height float,
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)	
as 
	insert into preparation_box values (
		@id,
		@name,
		@min_fill_height,
		@max_fill_height,
		@instance_status_id,
		@comment,		
		@create_date,
		@created_by,
		@update_date,
		@updated_by
	);
go

create proc csp_update_geometry
	@id uniqueidentifier,
	@name nvarchar(80),
	@min_fill_height float,
	@max_fill_height float,
	@instance_status_id int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@updated_by nvarchar(50)	
as 
	update preparation_box set 
		name = @name,
		min_fill_height_mm = @min_fill_height,
		max_fill_height_mm = @max_fill_height,
		instance_status_id = @instance_status_id,
		comment = @comment,		
		update_date = @update_date,
		updated_by = @updated_by
	where id = @id
go

create proc csp_select_geometries_flat
as
	select 
		pb.id,
		pb.name,
		pb.min_fill_height_mm, 
		pb.max_fill_height_mm, 
		st.name as 'instance_status',
		pb.comment,
		pb.create_date,
		pb.created_by,
		pb.update_date,
		pb.updated_by
	from preparation_box pb, instance_status st
	where pb.instance_status_id = st.id
	order by pb.name
go

/*===========================================================================*/
/* tbl preparation_method */

if OBJECT_ID('dbo.preparation_method', 'U') IS NOT NULL drop table preparation_method;

create table preparation_method (
	id uniqueidentifier primary key NOT NULL,
	name nvarchar(80) NOT NULL,
	description_link nvarchar(1024) default NULL,
	destructive bit default 0,	
	instance_status_id int default 1,
	comment nvarchar(1000) default NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl laboratory_x_preparation_method */

if OBJECT_ID('dbo.laboratory_x_preparation_method', 'U') IS NOT NULL drop table laboratory_x_preparation_method;

create table laboratory_x_preparation_method (
	laboratory_id uniqueidentifier NOT NULL,
	preparation_method_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl preparation */

if OBJECT_ID('dbo.preparation', 'U') IS NOT NULL drop table preparation;

create table preparation (
	id uniqueidentifier primary key NOT NULL,
	sample_id uniqueidentifier NOT NULL,
	assignment_id uniqueidentifier default NULL,
	laboratory_id uniqueidentifier NOT NULL,
	preparation_box_id uniqueidentifier NOT NULL,
	preparation_method_id uniqueidentifier NOT NULL,
	workflow_status_id int default 1,
	amount float default 0,
	prep_unit_id int default 1,		
	fill_height_mm float default 0,		
	instance_status_id int default 1,
	comment nvarchar(1000) default NULL,	
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl analysis_method */

if OBJECT_ID('dbo.analysis_method', 'U') IS NOT NULL drop table analysis_method;

create table analysis_method (
	id uniqueidentifier primary key NOT NULL,
	name nvarchar(32) NOT NULL,
	description_link nvarchar(1024) default NULL,
	specter_reference_regexp nvarchar(256) default NULL,	
	instance_status_id int default 1,
	comment nvarchar(1000) default NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl laboratory_x_analysis_method */

if OBJECT_ID('dbo.laboratory_x_analysis_method', 'U') IS NOT NULL drop table laboratory_x_analysis_method;

create table laboratory_x_analysis_method (
	laboratory_id uniqueidentifier NOT NULL,
	analysis_method_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl account_x_analysis_method */

if OBJECT_ID('dbo.account_x_analysis_method', 'U') IS NOT NULL drop table account_x_analysis_method;

create table account_x_analysis_method (
	account_id uniqueidentifier NOT NULL,
	analysis_method_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl analysis */

if OBJECT_ID('dbo.analysis', 'U') IS NOT NULL drop table analysis;

create table analysis (
	id uniqueidentifier primary key NOT NULL,
	assignment_id uniqueidentifier default NULL,
	laboratory_id uniqueidentifier NOT NULL,
	preparation_id uniqueidentifier NOT NULL,
	analysis_method_id uniqueidentifier NOT NULL,	
	workflow_status_id int default 1,
	specter_reference nvarchar(256) default NULL,
	activity_unit_id int NOT NULL,
	sigma float NOT NULL,
	nuclide_library nvarchar(256) default NULL,
	mda_library nvarchar(256) default NULL,	
	instance_status_id int default 1,
	comment nvarchar(1000) default NULL,	
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl decay_type */

if OBJECT_ID('dbo.decay_type', 'U') IS NOT NULL drop table decay_type;

create table decay_type (
	id int primary key NOT NULL,
	name nvarchar(16) unique NOT NULL
)
go

insert into decay_type (id, name) values(1, 'EC')
insert into decay_type (id, name) values(2, 'B+')
insert into decay_type (id, name) values(3, 'B-')
go

create proc csp_select_decay_type
	@id int
as 
	select * from decay_type where id = @id
go

/*===========================================================================*/
/* tbl nuclide */

if OBJECT_ID('dbo.nuclide', 'U') IS NOT NULL drop table nuclide;

create table nuclide (
	id uniqueidentifier primary key NOT NULL,
	name nvarchar(16) unique NOT NULL,
	proton_count int NOT NULL,
	neutron_count int NOT NULL,
	half_life_year float NOT NULL,
	half_life_uncertainty float NOT NULL,
	decay_type_id int NOT NULL,
	kxray_energy float NOT NULL,
	fluorescence_yield float NOT NULL,	
	instance_status_id int default 1,
	comment nvarchar(1000) NOT NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_insert_nuclide
	@id uniqueidentifier,
	@name nvarchar(16),
	@proton_count int,
	@neutron_count int,
	@half_life_year float,
	@half_life_uncertainty float,
	@decay_type_id int,
	@kxray_energy float,
	@fluorescence_yield float,
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)
as 
	insert into nuclide values (
		@id, 
		@name, 
		@proton_count, 
		@neutron_count, 
		@half_life_year, 
		@half_life_uncertainty, 
		@decay_type_id, 
		@kxray_energy,
		@fluorescence_yield,
		@instance_status_id,
		@comment,
		@create_date,
		@created_by,
		@update_date,
		@updated_by
	);
go

create proc csp_update_nuclide
	@id uniqueidentifier,
	@name nvarchar(16),
	@proton_count int,
	@neutron_count int,
	@half_life_year float,
	@half_life_uncertainty float,
	@decay_type_id int,
	@kxray_energy float,
	@fluorescence_yield float,
	@instance_status_id int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@updated_by nvarchar(50)
as 
	update nuclide set
		name = @name, 
		proton_count = @proton_count, 
		neutron_count = @neutron_count, 
		half_life_year = @half_life_year, 
		half_life_uncertainty = @half_life_uncertainty, 
		decay_type_id = @decay_type_id, 
		kxray_energy = @kxray_energy,
		fluorescence_yield = @fluorescence_yield,
		instance_status_id = @instance_status_id,
		comment = @comment,		
		update_date = @update_date,
		updated_by = @updated_by
	where id = @id
go

create proc csp_select_nuclides
as 
	select * from nuclide order by name
go

create proc csp_select_nuclide
	@id uniqueidentifier
as 
	select * from nuclide where id = @id
go

create proc csp_select_nuclides_flat
as
	select 
		n.id,
		n.name,
		n.proton_count, 
		n.neutron_count, 
		n.half_life_year, 
		n.half_life_uncertainty, 
		dt.name as 'decay_type', 
		n.kxray_energy,
		n.fluorescence_yield,
		n.comment,
		n.create_date,
		n.created_by,
		n.update_date,
		n.updated_by
	from nuclide n, decay_type dt where n.decay_type_id = dt.id
	order by n.name
go

/*===========================================================================*/
/* tbl nuclide_transmission */

if OBJECT_ID('dbo.nuclide_transmission', 'U') IS NOT NULL drop table nuclide_transmission;

create table nuclide_transmission (
	id uniqueidentifier primary key NOT NULL,
	nuclide_id uniqueidentifier NOT NULL,
	transmission_from int NOT NULL,
	transmission_to int NOT NULL,
	energy float NOT NULL,
	energy_uncertainty float NOT NULL,
	intensity float NOT NULL,
	intensity_uncertainty float NOT NULL,
	probability_of_decay float NOT NULL,
	probability_of_decay_uncertainty float NOT NULL,
	total_internal_conversion float NOT NULL,
	kshell_conversion float NOT NULL,	
	instance_status_id int default 1,
	comment nvarchar(1000) NOT NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_insert_energy_line
	@id uniqueidentifier,
	@nuclide_id uniqueidentifier,
	@transmission_from int,
	@transmission_to int,
	@energy float,
	@energy_uncertainty float,
	@intensity float,
	@intensity_uncertainty float,
	@probability_of_decay float,
	@probability_of_decay_uncertainty float,
	@total_internal_conversion float,
	@kshell_conversion float,
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)	
as 
	insert into nuclide_transmission values (
		@id,
		@nuclide_id,
		@transmission_from,
		@transmission_to,
		@energy,
		@energy_uncertainty,
		@intensity,
		@intensity_uncertainty,
		@probability_of_decay,
		@probability_of_decay_uncertainty,
		@total_internal_conversion,
		@kshell_conversion,
		@instance_status_id,
		@comment,
		@create_date,
		@created_by,
		@update_date,
		@updated_by
	);
go

create proc csp_update_energy_line
	@id uniqueidentifier,	
	@transmission_from int,
	@transmission_to int,
	@energy float,
	@energy_uncertainty float,
	@intensity float,
	@intensity_uncertainty float,
	@probability_of_decay float,
	@probability_of_decay_uncertainty float,
	@total_internal_conversion float,
	@kshell_conversion float,
	@instance_status_id int,
	@comment nvarchar(1000),
	@update_date datetime,
	@updated_by nvarchar(50)
as 
	update nuclide_transmission set 
		transmission_from = @transmission_from,
		transmission_to = @transmission_to,
		energy = @energy,
		energy_uncertainty = @energy_uncertainty,
		intensity = @intensity,
		intensity_uncertainty = @intensity_uncertainty,
		probability_of_decay = @probability_of_decay,
		probability_of_decay_uncertainty = @probability_of_decay_uncertainty,
		total_internal_conversion = @total_internal_conversion,
		kshell_conversion = @kshell_conversion,
		instance_status_id = @instance_status_id,
		comment = @comment,
		update_date = @update_date,
		updated_by = @updated_by
	where id = @id
go

create proc csp_select_energy_lines
as 
	select * from nuclide_transmission order by transmission_from
go

create proc csp_select_energy_lines_flat
as 
	select 
		nt.id,
		n.name as 'nuclide_name',
		nt.transmission_from,
		nt.transmission_to,
		nt.energy,
		nt.energy_uncertainty,
		nt.intensity,
		nt.intensity_uncertainty,
		nt.probability_of_decay,
		nt.probability_of_decay_uncertainty,
		nt.total_internal_conversion,
		nt.kshell_conversion,
		st.name as 'instance_status',
		nt.comment,
		nt.create_date,
		nt.created_by,
		nt.update_date,
		nt.updated_by
	from nuclide_transmission nt, nuclide n, instance_status st
	where nt.nuclide_id = n.id and nt.instance_status_id = st.id
	order by n.name, nt.transmission_from
go

create proc csp_select_energy_line
	@id uniqueidentifier
as 
	select * from nuclide_transmission where id = @id
go

create proc csp_select_energy_lines_for_nuclide
	@nuclide_id uniqueidentifier
as 
	select 
		id,	
		nuclide_id,
		transmission_from,
		transmission_to,
		energy,
		energy_uncertainty,
		intensity,
		intensity_uncertainty,
		probability_of_decay,
		probability_of_decay_uncertainty,
		total_internal_conversion,
		kshell_conversion,
		instance_status_id,
		comment,	
		create_date,
		created_by,
		update_date,
		updated_by
	from nuclide_transmission
	where nuclide_id = @nuclide_id
	order by transmission_from
go

/*===========================================================================*/
/* tbl project */

if OBJECT_ID('dbo.project', 'U') IS NOT NULL drop table project;

create table project (
	id uniqueidentifier primary key NOT NULL,
	parent_id uniqueidentifier default NULL,
	name nvarchar(256) NOT NULL,
	instance_status_id int default 1,
	comment nvarchar(1000) default NULL,		
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_select_project
	@id uniqueidentifier	
as 
	select * from project where id = @id
go

create proc csp_select_main_projects
as 
	select * from project where parent_id is NULL order by name
go

create proc csp_select_main_projects_short
as 
	select id, name	from project where parent_id is NULL order by name
go

create proc csp_select_sub_projects
	@parent_id uniqueidentifier
as 
	select * from project where parent_id = @parent_id order by name
go

create proc csp_select_sub_projects_for_main_project
	@parent_id uniqueidentifier
as 
	select * from project where parent_id = @parent_id order by name
go

create proc csp_select_sub_projects_short
	@parent_id uniqueidentifier	
as 
	select id, name from project where parent_id = @parent_id order by name
go

create proc csp_insert_project
	@id uniqueidentifier,
	@name nvarchar(256),	
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)
as 
	insert into project values (
		@id,
		NULL,
		@name, 				
		@instance_status_id,
		@comment,		
		@create_date,
		@created_by,
		@update_date,
		@updated_by
	);
go

create proc csp_update_project
	@id uniqueidentifier,
	@name nvarchar(256),	
	@instance_status_id int,
	@comment nvarchar(1000),
	@update_date datetime,
	@updated_by nvarchar(50)
as 
	update project set
		name = @name, 				
		instance_status_id = @instance_status_id,
		comment = @comment,				
		update_date = @update_date,
		updated_by = @updated_by
	where id = @id
go

create proc csp_insert_sub_project
	@id uniqueidentifier,
	@parent_id uniqueidentifier,
	@name nvarchar(256),	
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)
as 
	insert into project values (
		@id,
		@parent_id,
		@name, 				
		@instance_status_id,
		@comment,		
		@create_date,
		@created_by,
		@update_date,
		@updated_by
	);
go

/*===========================================================================*/
/* tbl project_x_account */

if OBJECT_ID('dbo.project_x_account', 'U') IS NOT NULL drop table project_x_account;

create table project_x_account (
	project_id uniqueidentifier NOT NULL,
	account_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl station */

if OBJECT_ID('dbo.station', 'U') IS NOT NULL drop table station;

create table station (
	id uniqueidentifier primary key NOT NULL,
	name nvarchar(128) unique NOT NULL,
	latitude float default 0,
	longitude float default 0,
	altitude float default 0,	
	instance_status_id int default 1,
	comment nvarchar(1000) default NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_select_station
	@id uniqueidentifier
as 
	select * from station where id = @id
go

create proc csp_insert_station
	@id uniqueidentifier,	
	@name nvarchar(80),
	@latitude float,	
	@longitude float,	
	@altitude float,	
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)	
as 
	insert into station values (
		@id,		
		@name,
		@latitude,		
		@longitude,		
		@altitude,		
		@instance_status_id,
		@comment,
		@create_date,
		@created_by,
		@update_date,
		@updated_by
	);
go

create proc csp_update_station
	@id uniqueidentifier,	
	@name nvarchar(80),
	@latitude float,	
	@longitude float,	
	@altitude float,	
	@instance_status_id int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@updated_by nvarchar(50)	
as 
	update station set 
		name = @name,
		latitude = @latitude,	
		longitude = @longitude,	
		altitude = @altitude,	
		instance_status_id = @instance_status_id,
		comment = @comment,	
		update_date = @update_date,
		updated_by = @updated_by
	where id = @id
go

create proc csp_select_stations_flat
as
	select 
		id,
		name,
		latitude, 	
		longitude, 	
		altitude, 	
		instance_status_id,	
		comment,	
		create_date,
		created_by,
		update_date,
		updated_by
	from station
	order by name
go

/*===========================================================================*/
/* tbl sample_type */

if OBJECT_ID('dbo.sample_type', 'U') IS NOT NULL drop table sample_type;

create table sample_type (
	id uniqueidentifier primary key NOT NULL,
	name nvarchar(256) unique NOT NULL,		
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_select_sample_types_short
as
	select 
		id,
		name
	from sample_type
	order by name
go

/*===========================================================================*/
/* tbl sample_storage */

if OBJECT_ID('dbo.sample_storage', 'U') IS NOT NULL drop table sample_storage;

create table sample_storage (
	id uniqueidentifier primary key NOT NULL,
	name nvarchar(256) unique NOT NULL,
	address nvarchar(1000) default NULL,	
	instance_status_id int default 1,
	comment nvarchar(1000) default NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_select_sample_storage
	@id uniqueidentifier
as 
	select * from sample_storage where id = @id
go

create proc csp_select_sample_storages
as
	select * from sample_storage order by name
go

create proc csp_select_sample_storages_flat
as
	select 
		s.id,
		s.name,
		s.address,
		st.name as 'instance_status',
		s.comment,	
		s.create_date,
		s.created_by,
		s.update_date,
		s.updated_by
	from sample_storage s, instance_status st
	order by s.name
go

create proc csp_insert_sample_storage
	@id uniqueidentifier,	
	@name nvarchar(80),
	@address nvarchar(1000),
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@created_by nvarchar(50),
	@update_date datetime,
	@updated_by nvarchar(50)	
as 
	insert into sample_storage values (
		@id,		
		@name,
		@address,				
		@instance_status_id,
		@comment,
		@create_date,
		@created_by,
		@update_date,
		@updated_by
	);
go

create proc csp_update_sample_storage
	@id uniqueidentifier,	
	@name nvarchar(80),
	@address nvarchar(1000),		
	@instance_status_id int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@updated_by nvarchar(50)	
as 
	update sample_storage set 
		name = @name,
		address = @address,			
		instance_status_id = @instance_status_id,
		comment = @comment,	
		update_date = @update_date,
		updated_by = @updated_by
	where id = @id
go

/*===========================================================================*/
/* tbl sample_component */

if OBJECT_ID('dbo.sample_component', 'U') IS NOT NULL drop table sample_component;

create table sample_component (
	id uniqueidentifier primary key NOT NULL,
	sample_type_id uniqueidentifier NOT NULL,
	name nvarchar(80) NOT NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

create proc csp_select_sample_components_for_sample_type
	@sample_type_id uniqueidentifier
as
	select 
		id,
		name		
	from sample_component
	where sample_type_id = @sample_type_id
	order by name
go

/*===========================================================================*/
/* tbl sample_parameter */

if OBJECT_ID('dbo.sample_parameter', 'U') IS NOT NULL drop table sample_parameter;

create table sample_parameter (
	id uniqueidentifier primary key NOT NULL,
	sample_type_id uniqueidentifier NOT NULL,	
	name nvarchar(80) NOT NULL,
	type nvarchar(30) NOT NULL,			
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl sample */

if OBJECT_ID('dbo.sample', 'U') IS NOT NULL drop table sample;

create table sample (
	id uniqueidentifier primary key NOT NULL,
	laboratory_id uniqueidentifier NOT NULL,	
	sample_type_id uniqueidentifier NOT NULL,	
	sample_storage_id uniqueidentifier default NULL,
	sample_component_id uniqueidentifier default NULL,
	project2_id uniqueidentifier NOT NULL,
	station_id uniqueidentifier default NULL,
	sampler_id uniqueidentifier default NULL,
	transform_from_id uniqueidentifier default NULL,	
	transform_to_id uniqueidentifier default NULL,	
	current_order_id uniqueidentifier default NULL,
	imported_from nvarchar(128) default NULL,
	imported_from_id nvarchar(128) default NULL,	
	latitude float default 0,
	longitude float default 0,
	altitude float default 0,
	community nvarchar(256) default NULL,
	location_type nvarchar(50) default NULL,
	location nvarchar(128) default NULL,	
	sampling_date_from datetime NOT NULL,
	sampling_date_to datetime default NULL,
	reference_date datetime NOT NULL,
	wet_weight_g float default NULL,	
	dry_weight_g float default NULL,
	volume_l float default NULL,
	lod_weight_start float default NULL,	
	lod_weight_end float default NULL,	
	lod_temperature float default NULL,
	confidential bit default 0,	
	parameters nvarchar(4000) default NULL,
	instance_status_id int default 1,
	comment nvarchar(1000) default NULL,	
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL
)
go

/*===========================================================================*/
/* tbl sample_type_x_preparation_method */

if OBJECT_ID('dbo.sample_type_x_preparation_method', 'U') IS NOT NULL drop table sample_type_x_preparation_method;

create table sample_type_x_preparation_method (
	sample_type_id uniqueidentifier NOT NULL,
	preparation_method_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl preparation_method_x_analysis_method */

if OBJECT_ID('dbo.preparation_method_x_analysis_method', 'U') IS NOT NULL drop table preparation_method_x_analysis_method;

create table preparation_method_x_analysis_method (	
	preparation_method_id uniqueidentifier NOT NULL,
	analysis_method_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl analysis_method_x_nuclide */

if OBJECT_ID('dbo.analysis_method_x_nuclide', 'U') IS NOT NULL drop table analysis_method_x_nuclide;

create table analysis_method_x_nuclide (		
	analysis_method_id uniqueidentifier NOT NULL,
	nuclide_id uniqueidentifier NOT NULL
)
go

/*===========================================================================*/
/* tbl analysis_result */

if OBJECT_ID('dbo.analysis_result', 'U') IS NOT NULL drop table analysis_result;

create table analysis_result (
	id uniqueidentifier primary key NOT NULL,
	analysis_id uniqueidentifier NOT NULL,
	nuclide_id uniqueidentifier NOT NULL,	
	workflow_status_id int default 1,
	activity float default NULL,
	activity_unit_id uniqueidentifier NOT NULL,
	activity_uncertainty float NOT NULL,
	activity_uncertainty_abs bit NOT NULL,		
	activity_approved bit default 0,
	uniform_activity float default NULL,
	uniform_activity_unit_id uniqueidentifier NOT NULL,		
	detection_limit float default NULL,
	detection_limit_approved bit default 0,
	instance_status_id int default 1,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL,
	update_date datetime NOT NULL,
	updated_by nvarchar(50) NOT NULL	
)
go

/*===========================================================================*/
/* tbl attachment */

if OBJECT_ID('dbo.attachment', 'U') IS NOT NULL drop table attachment;

create table attachment (
	id uniqueidentifier primary key NOT NULL,
	source_table nvarchar(80) NOT NULL,
	source_id uniqueidentifier NOT NULL,
	name nvarchar(256) NOT NULL,
	comment nvarchar(1000) default NULL,
	file_extension nvarchar(16) NOT NULL,
	value varbinary(max) NOT NULL,
	create_date datetime NOT NULL,
	created_by nvarchar(50) NOT NULL	
)
go