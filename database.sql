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

use dsa_lims
go

/*===========================================================================*/
/* tbl instance_status */

if OBJECT_ID('dbo.instance_status', 'U') is not null drop table instance_status;

create table instance_status (
	id int primary key not null,
	name nvarchar(20) not null	
)
go

insert into instance_status values(1, 'Active')
insert into instance_status values(2, 'Inactive')
insert into instance_status values(3, 'Deleted')
go

create proc csp_select_instance_status	
as 
	select * 
	from instance_status
	order by id
go

/*===========================================================================*/
/* tbl workflow_status */

if OBJECT_ID('dbo.workflow_status', 'U') is not null drop table workflow_status;

create table workflow_status (
	id int primary key not null,
	name nvarchar(20) not null	
)
go

insert into workflow_status values(1, 'Construction')
insert into workflow_status values(2, 'Complete')
insert into workflow_status values(3, 'Rejected')
go

create proc csp_select_workflow_status
as 
	select * 
	from workflow_status
	order by id
go

/*===========================================================================*/
/* tbl sigma_values */

if OBJECT_ID('dbo.sigma_values', 'U') is not null drop table sigma_values;

create table sigma_values (
	value float not null,
	name nvarchar(20) not null	
)
go

insert into sigma_values values(1.0, '1 (68.3%)')
insert into sigma_values values(1.96, '1.96 (95%)')
insert into sigma_values values(2.0, '2 (95.5%)')
insert into sigma_values values(3.0, '3 (99.9%)')
go

create proc csp_select_sigma_values
as 
	select * 
	from sigma_values
	order by value
go

/*===========================================================================*/
/* tbl sigma_mda_values */

if OBJECT_ID('dbo.sigma_mda_values', 'U') is not null drop table sigma_mda_values;

create table sigma_mda_values (
	value float not null,
	name nvarchar(20) not null	
)
go

insert into sigma_mda_values values(1.0, '1 (84.1%)')
insert into sigma_mda_values values(1.645, '1.645 (95%)')
insert into sigma_mda_values values(2.0, '2 (97.2%)')
insert into sigma_mda_values values(3.0, '3 (99.95%)')
go

create proc csp_select_sigma_mda_values
as 
	select * 
	from sigma_mda_values
	order by value
go

/*===========================================================================*/
/* tbl audit_log */

if OBJECT_ID('dbo.audit_log', 'U') is not null drop table audit_log;

create table audit_log (
	id uniqueidentifier primary key not null,
	source_table nvarchar(50) not null,
	source_id uniqueidentifier not null,	
	operation nvarchar(50) not null,
	comment nvarchar(200) default null,
	value nvarchar(max) not null,
	create_date datetime not null
)
go

create proc csp_insert_audit_message
	@id uniqueidentifier,
	@source_table nvarchar(50),
	@source_id uniqueidentifier,
	@operation nvarchar(50),
	@comment nvarchar(200),
	@value nvarchar(max),
	@create_date datetime
as
	insert into audit_log values(@id, @source_table, @source_id, @operation, @comment, @value, @create_date);
go

/*===========================================================================*/
/* tbl counters */

if OBJECT_ID('dbo.counters', 'U') is not null drop table counters;

create table counters (	
	name nvarchar(50) primary key not null,
	value int default 1	
)
go

insert into counters (name) values('database_version')
insert into counters (name) values('sample_counter')
go

/*===========================================================================*/
/* tbl person */

if OBJECT_ID('dbo.person', 'U') is not null drop table person;

create table person (
	id uniqueidentifier primary key not null,
	name nvarchar(128) not null,	
	email nvarchar(80) default null,
	phone nvarchar(80) default null,
	address nvarchar(1000) default null,
	create_date datetime not null,
	update_date datetime not null
)
go

create proc csp_insert_person
	@id uniqueidentifier,
	@name nvarchar(128),
	@email nvarchar(80),
	@phone nvarchar(80),
	@address nvarchar(1000),
	@create_date datetime,	
	@update_date datetime
as 
	insert into person values (
		@id,
		@name,		
		@email,
		@phone,
		@address,
		@create_date,
		@update_date
	);
go

create proc csp_update_person
	@id uniqueidentifier,
	@name nvarchar(128),	
	@email nvarchar(80),
	@phone nvarchar(80),
	@address nvarchar(1000),
	@update_date datetime
as 
	update person set
		name = @name,
		email = @email,
		phone = @phone,
		address = @address,
		update_date = @update_date
	where id = @id
go

create proc csp_select_person
	@id uniqueidentifier
as 
	select * 
	from person
	where id = @id
go

create proc csp_select_persons	
as 
	select * 
	from person	where email is not NULL
	order by name
go

create proc csp_select_persons_short
as 
	select 
		id, 
		name + ' (' + email + ')' as 'name'
	from person	where email is not NULL
	order by name
go

/*===========================================================================*/
/* tbl role */

if OBJECT_ID('dbo.role', 'U') is not null drop table role;

create table role (	
	id uniqueidentifier primary key not null,
	name nvarchar(80) not null	
)
go

insert into role values(NEWID(), 'Laboratory Administrator')
insert into role values(NEWID(), 'Laboratory Operator')
insert into role values(NEWID(), 'Order Administrator')
insert into role values(NEWID(), 'Order Operator')
insert into role values(NEWID(), 'Sample Registrator')
insert into role values(NEWID(), 'Spectator')

go

/*===========================================================================*/
/* tbl account_x_role */

if OBJECT_ID('dbo.account_x_role', 'U') is not null drop table account_x_role;

create table account_x_role (	
	account_id uniqueidentifier not null,
	role_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl account */

if OBJECT_ID('dbo.account', 'U') is not null drop table account;

create table account (	
	id uniqueidentifier primary key not null,
	username nvarchar(50) unique not null,
	person_id uniqueidentifier not null,
	laboratory_id uniqueidentifier default null,	
	language_code nvarchar(8) default 'en',
	instance_status_id int not null default 1,
	password_hash binary(32) not null,
	create_date datetime not null,	
	update_date datetime not null
)
go

create proc csp_insert_account
	@id uniqueidentifier,
	@username nvarchar(50),
	@person_id uniqueidentifier,
	@laboratory_id uniqueidentifier,
	@language_code nvarchar(8),
	@instance_status_id int,
	@password_hash binary(32),
	@create_date datetime,	
	@update_date datetime
as 
	insert into account values (
		@id,		
		@username,
		@person_id,
		@laboratory_id,
		@language_code,
		@instance_status_id,
		@password_hash,
		@create_date,
		@update_date
	);
go

create proc csp_update_account
	@id uniqueidentifier,
	@laboratory_id uniqueidentifier,
	@language_code nvarchar(8),
	@instance_status_id int,	
	@update_date datetime
as 
	update account set
		laboratory_id = @laboratory_id,
		language_code = @language_code,
		instance_status_id = @instance_status_id,
		update_date = @update_date
	where id = @id
go

create view cv_account
as
	select		
		a.id,
		a.person_id,
		p.name,
		p.email,
		p.phone,
		p.address,
		a.username,
		a.laboratory_id,
		a.language_code,
		a.instance_status_id,
		a.password_hash,
		a.create_date,	
		a.update_date
	from account a
		inner join person p on a.person_id = p.id
go

create proc csp_select_account
	@id uniqueidentifier
as 
	select * 
	from cv_account
	where id = @id
go

create proc csp_select_accounts
	@instance_status_level int
as 
	select * 
	from cv_account
	where instance_status_id <= @instance_status_level and email is not NULL
	order by name
go

create proc csp_select_accounts_short
	@instance_status_level int
as 
	select
		id,
		name + ' (' + email + ')' as name
	from cv_account
	where instance_status_id <= @instance_status_level and email is not NULL
	order by name
go

create proc csp_select_accounts_flat
	@instance_status_level int
as 
	select
		av.id,
		av.name,
		av.email,
		av.phone,
		av.address,
		av.username,
		l.name as 'laboratory_name',
		av.language_code,
		st.name as 'instance_status_name',
		av.create_date,	
		av.update_date
	from cv_account av
		left outer join laboratory l on av.laboratory_id = l.id 
		inner join instance_status st on av.instance_status_id = st.id 
	where av.instance_status_id <= @instance_status_level and av.email is not NULL
	order by av.name
go

create proc csp_select_accounts_for_laboratory
	@laboratory_id uniqueidentifier,
	@instance_status_level int
as 
	select * 
	from cv_account 
	where laboratory_id = @laboratory_id and instance_status_id <= @instance_status_level and email is not NULL
	order by name
go

create proc csp_select_accounts_for_laboratory_short
	@laboratory_id uniqueidentifier,
	@instance_status_level int
as 
	select 
		id, 
		name + ' (' + email + ')' as name
	from cv_account 
	where laboratory_id = @laboratory_id and instance_status_id <= @instance_status_level and email is not NULL
	order by instance_status_id, name
go

/*===========================================================================*/
/* tbl company */

if OBJECT_ID('dbo.company', 'U') is not null drop table company;

create table company (
	id uniqueidentifier primary key not null,
	name nvarchar(128) not null,
	email nvarchar(80) default null,
	phone nvarchar(80) default null,
	address nvarchar(1000) default null,
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_company
	@id uniqueidentifier,
	@name nvarchar(128),
	@email nvarchar(80),
	@phone nvarchar(80),
	@address nvarchar(1000),
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as 
	insert into company values (
		@id,
		@name,
		@email,
		@phone,
		@address,
		@instance_status_id,
		@comment,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_company
	@id uniqueidentifier,
	@name nvarchar(128),
	@email nvarchar(80),
	@phone nvarchar(80),
	@address nvarchar(1000),
	@instance_status_id int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@update_id uniqueidentifier
as 
	update company set 
		name = @name,
		email = @email,
		phone = @phone,
		address = @address,
		instance_status_id = @instance_status_id,
		comment = @comment,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_company
	@id uniqueidentifier
as 
	select * 
	from company
	where id = @id
go

create proc csp_select_companies
	@instance_status_level int
as 
	select * 
	from company
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_companies_short
	@instance_status_level int
as 
	select 
		id, 
		name 
	from company
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_companies_flat
	@instance_status_level int
as 
	select 		
		c.id,
		c.name,
		c.email,
		c.phone,
		c.address,
		st.name as 'instance_status_name',
		c.comment,	
		c.create_date,
		c.create_id,
		c.update_date,
		c.update_id
	from company c
		inner join instance_status st on c.instance_status_id = st.id 
			and c.instance_status_id <= @instance_status_level
	order by c.name
go

/*===========================================================================*/
/* tbl customer */

if OBJECT_ID('dbo.customer', 'U') is not null drop table customer;

create table customer (
	id uniqueidentifier primary key not null,
	person_id uniqueidentifier not null,
	company_id uniqueidentifier default null,
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_customer
	@id uniqueidentifier,
	@person_id uniqueidentifier,
	@company_id uniqueidentifier,
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as 
	insert into customer values (
		@id,
		@person_id,
		@company_id,
		@instance_status_id,
		@comment,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_customer
	@id uniqueidentifier,	
	@company_id uniqueidentifier,
	@instance_status_id int,
	@comment nvarchar(1000),
	@update_date datetime,
	@update_id uniqueidentifier
as 
	update customer set 
		company_id = @company_id,
		instance_status_id = @instance_status_id,
		comment = @comment,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create view cv_customer
as
	select
		c.id,
		c.person_id,
		c.company_id,
		p.name as 'person_name',
		p.email as 'person_email',
		p.phone as 'person_phone',
		p.address as 'person_address',
		co.name as 'company_name',
		co.email as 'company_email',
		co.phone as 'company_phone',
		co.address as 'company_address',						
		c.instance_status_id,
		c.comment,
		c.create_date,	
		c.create_id,
		c.update_date,
		c.update_id
	from customer c 
		inner join person p on c.person_id = p.id
		left outer join company co on c.company_id = co.id
go

create proc csp_select_customer
	@id uniqueidentifier
as 
	select *
	from cv_customer
	where id = @id
go

create proc csp_select_customers
	@instance_status_level int
as 
	select *
	from cv_customer
	where instance_status_id <= @instance_status_level
	order by person_name
go

create proc csp_select_customers_short
	@instance_status_level int
as 
	select 
		id, 
		person_name + ' (' + person_email + ')' as 'name'
	from cv_customer
	where instance_status_id <= @instance_status_level
	order by person_name
go

create proc csp_select_customers_flat
	@instance_status_level int
as 
	select		
		cv.id,
		cv.person_name,
		cv.person_email,
		cv.person_phone,
		cv.person_address,		
		cv.company_name,
		cv.company_email,
		cv.company_phone,
		cv.company_address,		
		st.name as 'instance_status_name'
	from cv_customer cv 
		inner join instance_status st on cv.instance_status_id = st.id 
			and cv.instance_status_id <= @instance_status_level
	order by cv.person_name
go

/*===========================================================================*/
/* tbl sampler */

if OBJECT_ID('dbo.sampler', 'U') is not null drop table sampler;

create table sampler (
	id uniqueidentifier primary key not null,
	person_id uniqueidentifier not null,
	company_id uniqueidentifier default null,
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_sampler
	@id uniqueidentifier,
	@person_id uniqueidentifier,
	@company_id uniqueidentifier,
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as 
	insert into sampler values (
		@id,
		@person_id,
		@company_id,
		@instance_status_id,
		@comment,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_sampler
	@id uniqueidentifier,	
	@company_id uniqueidentifier,
	@instance_status_id int,
	@comment nvarchar(1000),
	@update_date datetime,
	@update_id uniqueidentifier
as 
	update sampler set 
		company_id = @company_id,
		instance_status_id = @instance_status_id,
		comment = @comment,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create view cv_sampler
as
	select
		s.id,
		s.person_id,		
		s.company_id,
		p.name as 'person_name',
		p.email as 'person_email',
		p.phone as 'person_phone',
		p.address as 'person_address',
		co.name as 'company_name',
		co.email as 'company_email',
		co.phone as 'company_phone',
		co.address as 'company_address',
		s.instance_status_id,
		s.comment,
		s.create_date,	
		s.create_id,
		s.update_date,
		s.update_id
	from sampler s 
		inner join person p on s.person_id = p.id
		left outer join company co on s.company_id = co.id
go

create proc csp_select_sampler
	@id uniqueidentifier
as 
	select *
	from cv_sampler
	where id = @id
go

create proc csp_select_samplers
	@instance_status_level int
as 
	select *
	from cv_sampler
	where instance_status_id <= @instance_status_level
	order by person_name
go

create proc csp_select_samplers_short
	@instance_status_level int
as 
	select
		id,
		person_name + ' (' + person_email + ')' as 'name'
	from cv_sampler
	where instance_status_id <= @instance_status_level
	order by instance_status_id, person_name
go

create proc csp_select_samplers_flat
	@instance_status_level int
as 
	select
		sv.id,
		sv.person_name,
		sv.person_email,
		sv.person_phone,
		sv.person_address,
		sv.company_name,
		sv.company_email,
		sv.company_phone,
		sv.company_address,
		st.name as 'instance_status_name'
	from cv_sampler sv 
		inner join instance_status st on sv.instance_status_id = st.id 
			and sv.instance_status_id <= @instance_status_level
	order by sv.person_name
go

/*===========================================================================*/
/* tbl sampling_method */

if OBJECT_ID('dbo.sampling_method', 'U') is not null drop table sampling_method;

create table sampling_method (
	id uniqueidentifier primary key not null,
	name nvarchar(256) not null,
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_sampling_method
	@id uniqueidentifier,
	@name nvarchar(256),
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as 
	insert into sampling_method values (
		@id,
		@name,		
		@instance_status_id,
		@comment,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_sampling_method
	@id uniqueidentifier,
	@name nvarchar(256),	
	@instance_status_id int,
	@comment nvarchar(1000),
	@update_date datetime,
	@update_id uniqueidentifier
as 
	update sampling_method set 
		name = @name,		
		instance_status_id = @instance_status_id,
		comment = @comment,	
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_sampling_methods
	@instance_status_level int
as 
	select * 
	from sampling_method
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_sampling_methods_short
	@instance_status_level int
as 
	select
		id,
		name
	from sampling_method
	where instance_status_id <= @instance_status_level
	order by instance_status_id, name
go

create proc csp_select_sampling_methods_flat
	@instance_status_level int
as 
	select 
		s.id,
		s.name,
		st.name as 'instance_status_name',
		s.comment,
		s.create_date,
		s.create_id,
		s.update_date,
		s.update_id
	 from sampling_method s, instance_status st
	 where s.instance_status_id = st.id and s.instance_status_id <= @instance_status_level
	 order by s.name
go

create proc csp_select_sampling_method
	@id uniqueidentifier
as 
	select * from sampling_method where id = @id
go

/*===========================================================================*/
/* tbl laboratory */

if OBJECT_ID('dbo.laboratory', 'U') is not null drop table laboratory;

create table laboratory (
	id uniqueidentifier primary key not null,
	name nvarchar(256) unique not null,
	name_prefix nvarchar(8) unique not null,
	address nvarchar(256) default null,
	email nvarchar(80) default null,
	phone nvarchar(80) default null,
	last_assignment_counter_year int default 2000,
	assignment_counter int default 1,
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,
	laboratory_logo varbinary(max) default null,
	accredited_logo varbinary(max) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_laboratory
	@id uniqueidentifier,
	@name nvarchar(256),
	@name_prefix nvarchar(8),
	@address nvarchar(256),
	@email nvarchar(80),
	@phone nvarchar(80),
	@last_assignment_counter_year int,
	@assignment_counter int,
	@instance_status_id int,
	@comment nvarchar(1000),	
	@laboratory_logo varbinary(max),
	@accredited_logo varbinary(max),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as 
	insert into laboratory values (
		@id,
		@name,
		@name_prefix,
		@address,
		@email,
		@phone,		
		@last_assignment_counter_year,
		@assignment_counter,
		@instance_status_id,
		@comment,		
		@laboratory_logo,
		@accredited_logo,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_laboratory
	@id uniqueidentifier,
	@name nvarchar(256),
	@name_prefix nvarchar(8),
	@address nvarchar(256),
	@email nvarchar(80),
	@phone nvarchar(80),	
	@instance_status_id int,
	@comment nvarchar(1000),
	@laboratory_logo varbinary(max),
	@accredited_logo varbinary(max),
	@update_date datetime,
	@update_id uniqueidentifier
as 
	update laboratory set
		id = @id,
		name = @name,
		name_prefix = @name_prefix,
		address = @address,
		email = @email,
		phone = @phone,	
		instance_status_id = @instance_status_id,
		comment = @comment,
		laboratory_logo = @laboratory_logo,
		accredited_logo = @accredited_logo,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_increment_assignment_counter
	@laboratory_id uniqueidentifier,
	@current_count int output
as 
	select @current_count = assignment_counter from laboratory where id = @laboratory_id;
	update laboratory set assignment_counter = @current_count + 1 where id = @laboratory_id;
	return
go

create proc csp_select_laboratories
	@instance_status_level int
as
	select * 
	from  laboratory 
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_laboratories_flat
	@instance_status_level int
as
	select
		l.id,
		l.name,
		l.name_prefix,
		l.address,
		l.email,
		l.phone,
		l.assignment_counter,
		st.name as 'instance_status_name',
		l.comment,			
		l.create_date,
		l.create_id,
		l.update_date,
		l.update_id
	from laboratory l, instance_status st
	where l.instance_status_id = st.id and l.instance_status_id <= @instance_status_level
	order by l.name
go

create proc csp_select_laboratories_short
	@instance_status_level int
as
	select 
		id, 
		name	
	from laboratory 
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_laboratory
	@id uniqueidentifier	
as 
	select * from laboratory where id = @id
go

/*===========================================================================*/
/* tbl preparation_unit */

if OBJECT_ID('dbo.preparation_unit', 'U') is not null drop table preparation_unit;

create table preparation_unit (
	id int primary key not null,
	name nvarchar(20) not null,
	name_short nvarchar(8) not null
)
go

insert into preparation_unit values(1, 'Wet weight (g)', 'g ww')
insert into preparation_unit values(2, 'Dry weight (g)', 'g dw')
insert into preparation_unit values(3, 'Ash weight (g)', 'g aw')
insert into preparation_unit values(4, 'Volume (L)', 'L vol')
insert into preparation_unit values(5, 'g', 'g')
go

create proc csp_select_preparation_units
as 
	select *
	from preparation_unit
	order by name
go

/*===========================================================================*/
/* tbl activity_unit */

if OBJECT_ID('dbo.activity_unit', 'U') is not null drop table activity_unit;

create table activity_unit (
	id uniqueidentifier primary key not null,
	name nvarchar(20) not null,
	convert_factor float default null,
	uniform_activity_unit_id int not null default 0
)
go

insert into activity_unit values(NEWID(), 'Bq', 1.0, 1)
insert into activity_unit values(NEWID(), 'Bq/g', 1.0, 2)
insert into activity_unit values(NEWID(), 'Bq/kg', 0.001, 2)
insert into activity_unit values(NEWID(), 'Bq/m2', 1.0, 3)
insert into activity_unit values(NEWID(), 'Bq/m3', 1.0, 4)
insert into activity_unit values(NEWID(), 'Bq/l', 1000.0, 4)
insert into activity_unit values(NEWID(), 'mBq/g', 1000.0, 2)
insert into activity_unit values(NEWID(), 'mBq/l', 1.0, 4)
insert into activity_unit values(NEWID(), 'Bq/filter', 1.0, 1)
insert into activity_unit values(NEWID(), 'Counts/cm2/s', 1.0, 5)
go

create proc csp_select_activity_units
as 
	select *
	from activity_unit
	order by name
go

create proc csp_select_activity_units_short
as 
	select 
		id,	
		name
	from activity_unit
	order by name
go

create proc csp_select_activity_units_flat
as 
	select 
		au.id,	
		au.name,
		au.convert_factor,
		uau.name as 'uniform_activity_unit_name'
	from activity_unit au, uniform_activity_unit uau 
	where au.uniform_activity_unit_id = uau.id
	order by au.name
go

/*===========================================================================*/
/* tbl activity_unit_type */

if OBJECT_ID('dbo.activity_unit_type', 'U') is not null drop table activity_unit_type;

create table activity_unit_type (
	id uniqueidentifier primary key not null,	
	name nvarchar(20) not null,
	name_short nvarchar(8) not null,
	name_report nvarchar(20) not null
)
go

insert into activity_unit_type values(NEWID(), 'Wet weight', 'ww', 'vv')
insert into activity_unit_type values(NEWID(), 'Dry weight', 'dw', 'tv')
insert into activity_unit_type values(NEWID(), 'Ash weight', 'aw', 'av')
go

create proc csp_select_activity_unit_types
as 
	select *		
	from activity_unit_type
	order by name
go

/*===========================================================================*/
/* tbl uniform_activity_unit */

if OBJECT_ID('dbo.uniform_activity_unit', 'U') is not null drop table uniform_activity_unit;

create table uniform_activity_unit (
	id int primary key not null,
	name nvarchar(20) not null	
)
go

insert into uniform_activity_unit values(1, 'Bq')
insert into uniform_activity_unit values(2, 'Bq/g')
insert into uniform_activity_unit values(3, 'Bq/m2')
insert into uniform_activity_unit values(4, 'Bq/m3')
insert into uniform_activity_unit values(5, 'Counts/cm2/s')
go

create proc csp_select_uniform_activity_units
as 
	select *
	from uniform_activity_unit
	order by id
go

/*===========================================================================*/
/* tbl quantity_unit */

if OBJECT_ID('dbo.quantity_unit', 'U') is not null drop table quantity_unit;

create table quantity_unit (
	id int primary key not null,
	name nvarchar(20) not null	
)
go

insert into quantity_unit values(1, 'L')
insert into quantity_unit values(2, 'cm2')
insert into quantity_unit values(3, 'm3')
insert into quantity_unit values(4, 'kg ww')
insert into quantity_unit values(5, 'kg dw')
insert into quantity_unit values(6, 'kg aw')
insert into quantity_unit values(7, 'g ww')
insert into quantity_unit values(8, 'g dw')
insert into quantity_unit values(9, 'g aw')
insert into quantity_unit values(10, 'Filter')
go

create proc csp_select_quantity_units
as 
	select *
	from quantity_unit
	order by id
go

/*===========================================================================*/
/* tbl accreditation_term */

if OBJECT_ID('dbo.accreditation_term', 'U') is not null drop table accreditation_term;

create table accreditation_term (
	id uniqueidentifier primary key not null,	
	density_min float default null,
	density_max float default null,
	dry_weight_min float default null,
	dry_weight_max float default null,
	wet_weight_min float default null,
	wet_weight_max float default null,
	ash_weight_min float default null,
	ash_weight_max float default null,
	volume_min float default null,
	volume_max float default null,
	fill_height_min float default null,
	fill_height_max float default null,
	instance_status_id int not null default 1,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl accreditation_term_x_laboratory */

if OBJECT_ID('dbo.accreditation_term_x_laboratory', 'U') is not null drop table accreditation_term_x_laboratory;

create table accreditation_term_x_laboratory (
	accreditation_term_id uniqueidentifier not null,
	laboratory_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl accreditation_term_x_sample_type */

if OBJECT_ID('dbo.accreditation_term_x_sample_type', 'U') is not null drop table accreditation_term_x_sample_type;

create table accreditation_term_x_sample_type (
	accreditation_term_id uniqueidentifier not null,
	sample_type_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl accreditation_term_x_preparation_method */

if OBJECT_ID('dbo.accreditation_term_x_preparation_method', 'U') is not null drop table accreditation_term_x_preparation_method;

create table accreditation_term_x_preparation_method (
	accreditation_term_id uniqueidentifier not null,
	preparation_method_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl accreditation_term_x_analysis_method */

if OBJECT_ID('dbo.accreditation_term_x_analysis_method', 'U') is not null drop table accreditation_term_x_analysis_method;

create table accreditation_term_x_analysis_method (
	accreditation_term_id uniqueidentifier not null,
	analysis_method_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl accreditation_term_x_nuclide */

if OBJECT_ID('dbo.accreditation_term_x_nuclide', 'U') is not null drop table accreditation_term_x_nuclide;

create table accreditation_term_x_nuclide (
	accreditation_term_id uniqueidentifier not null,
	nuclide_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl county */

if OBJECT_ID('dbo.county', 'U') is not null drop table county;

create table county (
	id uniqueidentifier primary key not null,	
	name nvarchar(128) unique not null,
	county_number int not null,	
	instance_status_id int not null default 1,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_county
	@id uniqueidentifier,
	@name nvarchar(80),
	@county_number int,	
	@instance_status_id int,
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	insert into county values (
		@id,		
		@name,		
		@county_number,		
		@instance_status_id,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_county
	@id uniqueidentifier,
	@name nvarchar(80),
	@county_number int,	
	@instance_status_id int,
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	update county set 
		name = @name,
		county_number = @county_number,		
		instance_status_id = @instance_status_id,		
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_county
	@id uniqueidentifier
as 
	select * from county where id = @id
go

create proc csp_select_counties
	@instance_status_level int
as
	select * 
	from county 
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_counties_short
	@instance_status_level int
as
	select
		id,
		name
	from county 
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_counties_flat
	@instance_status_level int
as
	select 
		c.id,
		c.name,
		c.county_number, 	
		st.name as 'instance_status_name',	
		c.create_date,
		c.create_id,
		c.update_date,
		c.update_id
	from county c, instance_status st
	where c.instance_status_id = st.id
	order by c.name
go

/*===========================================================================*/
/* tbl municipality */

if OBJECT_ID('dbo.municipality', 'U') is not null drop table municipality;

create table municipality (
	id uniqueidentifier primary key not null,
	county_id uniqueidentifier not null,
	name nvarchar(128) not null,
	municipality_number int not null,		
	instance_status_id int not null default 1,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_municipality
	@id uniqueidentifier,
	@county_id uniqueidentifier,
	@name nvarchar(80),
	@municipality_number int,	
	@instance_status_id int,
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	insert into municipality values (
		@id,		
		@county_id,
		@name,		
		@municipality_number,		
		@instance_status_id,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_municipality
	@id uniqueidentifier,
	@name nvarchar(80),
	@municipality_number int,	
	@instance_status_id int,
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	update municipality set 
		name = @name,
		municipality_number = @municipality_number,		
		instance_status_id = @instance_status_id,		
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_municipality
	@id uniqueidentifier
as 
	select * from municipality where id = @id
go

create proc csp_select_municipalities_for_county
	@county_id uniqueidentifier,
	@instance_status_level int
as 
	select *
	from municipality
	where county_id = @county_id and instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_municipalities_for_county_short
	@county_id uniqueidentifier,
	@instance_status_level int
as 
	select
		id,
		name
	from municipality
	where county_id = @county_id and instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_municipalities_for_county_flat
	@county_id uniqueidentifier,
	@instance_status_level int
as 
	select 
		m.id,	
		c.name as 'county_name',
		m.name,
		m.municipality_number,
		st.name as 'instance_status_name',
		m.create_date,
		m.create_id,
		m.update_date,
		m.update_id
	from municipality m, county c, instance_status st
	where m.county_id = @county_id and m.county_id = c.id and m.instance_status_id = st.id and m.instance_status_id <= @instance_status_level
	order by m.name
go

/*===========================================================================*/
/* tbl location_type */

if OBJECT_ID('dbo.location_type', 'U') is not null drop table location_type;

create table location_type (
	id int primary key not null,
	name nvarchar(32) not null	
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
	select *
	from location_type 
	order by id
go

/*===========================================================================*/
/* tbl assignment */

if OBJECT_ID('dbo.assignment', 'U') is not null drop table assignment;

create table assignment (
	id uniqueidentifier primary key not null,
	name nvarchar(80) not null,	
	laboratory_id uniqueidentifier not null,
	account_id uniqueidentifier not null,
	deadline datetime not null,
	requested_sigma_act float not null default 2.0,
	requested_sigma_mda float not null default 1.645,
	customer_company_name nvarchar(80) default null,	
	customer_company_email nvarchar(80) default null,
	customer_company_phone nvarchar(80) default null,
	customer_company_address nvarchar(256) default null,
	customer_contact_name nvarchar(80) default null,	
	customer_contact_email nvarchar(80) default null,
	customer_contact_phone nvarchar(80) default null,
	customer_contact_address nvarchar(256) default null,
	approved_customer bit default 0,
	approved_customer_by nvarchar(50) default null,
	approved_laboratory bit default 0,	
	approved_laboratory_by nvarchar(50) default null,
	content_comment nvarchar(1000) default null,
	report_comment nvarchar(1000) default null,
	audit_comment nvarchar(4000) default null,
	workflow_status_id int not null default 1,
	last_workflow_status_date datetime default null,
	last_workflow_status_by nvarchar(50) default null,
	analysis_report_version int not null default 1,
	instance_status_id int not null default 1,
	locked_id uniqueidentifier default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_assignment
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
	@update_id uniqueidentifier
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
		@update_id		
	);
go

create proc csp_update_assignment
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
	@update_id uniqueidentifier
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
		update_id = @update_id		
	where id = @id
go

create proc csp_update_assignment_analysis_report
	@id uniqueidentifier,	
	@audit_comment_stub nvarchar(100)
as 		
	update assignment set		
		audit_comment = audit_comment + CHAR(13) + CHAR(10) + @audit_comment_stub,
		analysis_report_version = analysis_report_version + 1
	where id = @id
go

create proc csp_select_assignment
	@id uniqueidentifier
as
	select *
	from assignment
	where id = @id
go

create proc csp_select_assignment_flat
	@id uniqueidentifier
as
	select		
		a.id,
		a.name,		
		l.name as 'laboratory_name',
		va.name as 'account_name',
		a.deadline,
		a.requested_sigma_act,
		a.requested_sigma_mda,
		a.customer_company_name,		
		a.customer_company_email,
		a.customer_company_phone,
		a.customer_company_address,
		a.customer_contact_name,		
		a.customer_contact_email,
		a.customer_contact_phone,
		a.customer_contact_address,
		a.approved_customer,
		a.approved_customer_by,
		a.approved_laboratory,	
		a.approved_laboratory_by,	
		a.content_comment,
		a.report_comment,		
		a.audit_comment,		
		a.workflow_status_id,
		a.last_workflow_status_date,
		a.last_workflow_status_by,
		a.analysis_report_version,
		insta.name as 'instance_status_name',
		(select name from cv_account where id = a.locked_id) as 'locked_name',
		a.create_date,
		a.create_id,
		a.update_date,
		a.update_id		
	from assignment a 		
		left outer join laboratory l on a.laboratory_id = l.id
		left outer join cv_account va on a.account_id = va.id
		inner join instance_status insta on a.instance_status_id = insta.id
	where a.id = @id	
go

create proc csp_select_assignment_informative
	@id uniqueidentifier
as
	select
		ass.name,
		lab.name as 'laboratory_name',
		acc.name as 'responsible_name',
		ass.customer_company_name,		
		ass.customer_contact_name,
		ass.create_date
	from assignment ass
		inner join laboratory lab on lab.id = ass.laboratory_id
		inner join cv_account acc on ass.account_id = acc.id
	where ass.id = @id
go

create proc csp_select_assignments
	@instance_status_level int
as
	select *
	from assignment
	where instance_status_id <= @instance_status_level
go

create proc csp_select_assignments_short
	@instance_status_level int
as
	select id, name
	from assignment
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_assignments_for_laboratory_short
	@lab_id uniqueidentifier,
	@instance_status_level int
as
	select id, name
	from assignment
	where instance_status_id <= @instance_status_level and laboratory_id = @lab_id
go

create proc csp_select_assignments_flat
	@instance_status_level int
as
	select		
		a.id,
		a.name,		
		l.name as 'laboratory_name',
		va.name as 'account_name',
		a.deadline,
		a.requested_sigma_act,
		a.requested_sigma_mda,
		a.customer_company_name,		
		a.customer_company_email,
		a.customer_company_phone,
		a.customer_company_address,
		a.customer_contact_name,		
		a.customer_contact_email,
		a.customer_contact_phone,
		a.customer_contact_address,
		a.approved_customer,
		a.approved_customer_by,
		a.approved_laboratory,	
		a.approved_laboratory_by,	
		a.content_comment,
		a.report_comment,		
		a.audit_comment,		
		a.workflow_status_id,
		a.last_workflow_status_date,
		a.last_workflow_status_by,
		a.analysis_report_version,
		insta.name as 'instance_status_name',
		(select name from cv_account where id = a.locked_id) as 'locked_name',		
		a.create_date,
		a.create_id,
		a.update_date,
		a.update_id		
	from assignment a 		
		left outer join laboratory l on a.laboratory_id = l.id
		left outer join cv_account va on a.account_id = va.id
		inner join instance_status insta on a.instance_status_id = insta.id
	where a.instance_status_id <= @instance_status_level
	order by a.create_date desc
go

/*===========================================================================*/
/* tbl assignment_sample_type */

if OBJECT_ID('dbo.assignment_sample_type', 'U') is not null drop table assignment_sample_type;

create table assignment_sample_type (
	id uniqueidentifier primary key not null,	
	assignment_id uniqueidentifier not null,	
	sample_type_id uniqueidentifier not null,	
	sample_component_id uniqueidentifier default null,	
	sample_count int not null,
	requested_activity_unit_id uniqueidentifier default null,
	requested_activity_unit_type_id uniqueidentifier default null,
	return_to_sender bit default 0,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_assignment_sample_type
	@id uniqueidentifier,	
	@assignment_id uniqueidentifier,	
	@sample_type_id uniqueidentifier,	
	@sample_component_id uniqueidentifier,	
	@sample_count int,
	@requested_activity_unit_id uniqueidentifier,
	@requested_activity_unit_type_id uniqueidentifier,
	@return_to_sender bit,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as	
	insert into assignment_sample_type values (
		@id,	
		@assignment_id,	
		@sample_type_id,	
		@sample_component_id,	
		@sample_count,
		@requested_activity_unit_id,
		@requested_activity_unit_type_id,
		@return_to_sender,
		@comment,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_assignment_sample_type
	@id uniqueidentifier,	
	@assignment_id uniqueidentifier,	
	@sample_type_id uniqueidentifier,	
	@sample_component_id uniqueidentifier,	
	@sample_count int,
	@requested_activity_unit_id uniqueidentifier,
	@requested_activity_unit_type_id uniqueidentifier,
	@return_to_sender bit,
	@comment nvarchar(1000),	
	@update_date datetime,
	@update_id uniqueidentifier
as	
	update assignment_sample_type set 
		assignment_id = @assignment_id,	
		sample_type_id = @sample_type_id,	
		sample_component_id = @sample_component_id,	
		sample_count = @sample_count,
		requested_activity_unit_id = @requested_activity_unit_id,
		requested_activity_unit_type_id = @requested_activity_unit_type_id,
		return_to_sender = @return_to_sender,
		comment = @comment,		
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_assignment_sample_type
	@id uniqueidentifier
as
	select *
	from assignment_sample_type		
	where id = @id
go

create proc csp_select_assignment_sample_type_flat
	@id uniqueidentifier
as
	select 
		ast.id as 'id', 
		ass.name as 'assignment_name',
		st.name as 'sample_type_name', 
		sc.name as 'sample_component_name', 
		ast.sample_count as 'sample_count',
		rau.name as 'requested_activity_unit_name',
		raut.name as 'requested_activity_unit_type_name',
		ast.return_to_sender as 'return_to_sender',
		ast.comment as 'comment',
		ast.create_date,
		ast.create_id,
		ast.update_date,
		ast.update_id
	from assignment_sample_type ast
		inner join assignment ass on ass.id = ast.assignment_id
		inner join sample_type st on ast.sample_type_id = st.id
		left outer join sample_component sc on ast.sample_component_id = sc.id
		left outer join requested_activity_unit rau on rau.id = ast.requested_activity_unit_id
		left outer join requested_activity_unit_type raut on raut.id = ast.requested_activity_unit_type_id
	where ast.id = @id
go

create proc csp_select_assignment_sample_types
	@assignment_id uniqueidentifier
as
	select 
	ast.id as 'id', 
	ast.sample_type_id as 'sample_type_id', 
	ast.sample_count as 'sample_count',
	st.name as 'sample_type_name', 	
	sc.name as 'sample_component_name',
	ast.return_to_sender as 'return_to_sender',
	ast.comment as 'sample_comment'
from assignment_sample_type ast
	inner join sample_type st on ast.sample_type_id = st.id and ast.assignment_id = @assignment_id
	left outer join sample_component sc on ast.sample_component_id = sc.id
	order by st.name
go

create proc csp_select_assignment_sample_types_for_sample_type
	@assignment_id uniqueidentifier,
	@sample_type_id uniqueidentifier
as
	select 
	ast.id as 'id', 
	ast.sample_type_id as 'sample_type_id', 
	ast.sample_count as 'sample_count',
	st.name as 'sample_type_name', 	
	sc.name as 'sample_component_name',
	ast.return_to_sender as 'return_to_sender',
	ast.comment as 'sample_comment'
from assignment_sample_type ast
	inner join sample_type st on ast.sample_type_id = st.id and ast.assignment_id = @assignment_id and st.id = @sample_type_id
	left outer join sample_component sc on ast.sample_component_id = sc.id
	order by st.name
go

create proc csp_select_assignment_sample_types_for_sample_type_name
	@assignment_id uniqueidentifier,
	@sample_type_name nvarchar(256)
as
	select
		ast.id as 'id', 
		ast.sample_type_id as 'sample_type_id', 
		ast.sample_count as 'sample_count',
		st.name as 'sample_type_name', 	
		sc.name as 'sample_component_name',
		ast.comment as 'sample_comment'
	from assignment_sample_type ast
		inner join sample_type st on ast.sample_type_id = st.id 
			and ast.assignment_id = @assignment_id
			and @sample_type_name like st.path + '%'
		left outer join sample_component sc on ast.sample_component_id = sc.id
	order by st.name
go

/*===========================================================================*/
/* tbl assignment_preparation_method */

if OBJECT_ID('dbo.assignment_preparation_method', 'U') is not null drop table assignment_preparation_method;

create table assignment_preparation_method (
	id uniqueidentifier primary key not null,		
	assignment_sample_type_id uniqueidentifier not null,				
	preparation_method_id uniqueidentifier default null,
	preparation_method_count int not null,
	preparation_laboratory_id uniqueidentifier default null,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_assignment_preparation_method
	@id uniqueidentifier,	
	@assignment_sample_type_id uniqueidentifier,	
	@preparation_method_id uniqueidentifier,
	@preparation_method_count int,
	@preparation_laboratory_id uniqueidentifier,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as	
	insert into assignment_preparation_method values (
		@id,	
		@assignment_sample_type_id,	
		@preparation_method_id,
		@preparation_method_count,
		@preparation_laboratory_id,
		@comment,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_assignment_preparation_method
	@id uniqueidentifier,	
	@assignment_sample_type_id uniqueidentifier,	
	@preparation_method_id uniqueidentifier,
	@preparation_method_count int,
	@preparation_laboratory_id uniqueidentifier,
	@comment nvarchar(1000),	
	@update_date datetime,
	@update_id uniqueidentifier
as	
	update assignment_preparation_method set		
		assignment_sample_type_id = @assignment_sample_type_id,	
		preparation_method_id = @preparation_method_id,
		preparation_method_count = @preparation_method_count,
		preparation_laboratory_id = @preparation_laboratory_id,
		comment = @comment,		
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_assignment_preparation_method
	@id uniqueidentifier
as	
	select *
	from assignment_preparation_method		
	where id = @id
go

create proc csp_select_assignment_preparation_method_flat
	@id uniqueidentifier
as	
	select 
		apm.id,	
		apm.assignment_sample_type_id,	
		pm.name as 'preparation_method_name',
		apm.preparation_method_count as 'preparation_method_count',
		lab.name as 'preparation_laboratory_name',
		apm.comment,
		apm.create_date,
		apm.create_id,
		apm.update_date,
		apm.update_id
	from assignment_preparation_method apm 		
		inner join preparation_method pm on pm.id = apm.preparation_method_id		
		left outer join laboratory lab on apm.preparation_laboratory_id = lab.id
	where apm.id = @id
go

create proc csp_select_assignment_preparation_methods
	@assignment_sample_type_id uniqueidentifier
as	
	select 
		apm.id as 'id',
		apm.preparation_method_count as 'count', 
		apm.comment as 'comment', 
		apm.preparation_laboratory_id as 'preparation_laboratory_id', 
		lab.name as 'preparation_laboratory_name', 
		pm.name_short as 'preparation_method_name',
		pm.name as 'preparation_method_name_full'
	from assignment_preparation_method apm 
		inner join preparation_method pm on apm.assignment_sample_type_id = @assignment_sample_type_id and apm.preparation_method_id = pm.id
		left outer join laboratory lab on apm.preparation_laboratory_id = lab.id
	order by pm.name_short
go

/*===========================================================================*/
/* tbl assignment_analysis_method */

if OBJECT_ID('dbo.assignment_analysis_method', 'U') is not null drop table assignment_analysis_method;

create table assignment_analysis_method (
	id uniqueidentifier primary key not null,		
	assignment_preparation_method_id uniqueidentifier not null,				
	analysis_method_id uniqueidentifier default null,
	analysis_method_count int not null,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_assignment_analysis_method
	@id uniqueidentifier,	
	@assignment_preparation_method_id uniqueidentifier,	
	@analysis_method_id uniqueidentifier,
	@analysis_method_count int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as	
	insert into assignment_analysis_method values (
		@id,	
		@assignment_preparation_method_id,	
		@analysis_method_id,
		@analysis_method_count,
		@comment,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_assignment_analysis_method
	@id uniqueidentifier,	
	@assignment_preparation_method_id uniqueidentifier,	
	@analysis_method_id uniqueidentifier,
	@analysis_method_count int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@update_id uniqueidentifier
as	
	update assignment_analysis_method set		
		assignment_preparation_method_id = @assignment_preparation_method_id,	
		analysis_method_id = @analysis_method_id,
		analysis_method_count = @analysis_method_count,
		comment = @comment,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_assignment_analysis_method
	@id uniqueidentifier
as	
	select *
	from assignment_analysis_method		
	where id = @id
go

create proc csp_select_assignment_analysis_method_flat
	@id uniqueidentifier
as	
	select 
		aam.id as 'id', 
		aam.assignment_preparation_method_id,
		am.name as 'analysis_method_name',
		aam.analysis_method_count as 'analysis_method_count',
		aam.comment,
		aam.create_date,
		aam.create_id,
		aam.update_date,
		aam.update_id
	from assignment_analysis_method aam			
		inner join analysis_method am on am.id = aam.analysis_method_id
	where aam.id = @id
go

create proc csp_select_assignment_analysis_methods
	@assignment_preparation_method_id uniqueidentifier
as	
	select 
		aam.id as 'id', 
		aam.analysis_method_count as 'count', 
		aam.comment as 'comment', 
		am.name_short as 'analysis_method_name',
		am.name as 'analysis_method_name_full'
	from assignment_analysis_method aam, analysis_method am
	where aam.assignment_preparation_method_id = @assignment_preparation_method_id and aam.analysis_method_id = am.id
	order by am.name_short
go

/*===========================================================================*/
/* tbl assignment_x_account */

if OBJECT_ID('dbo.assignment_x_account', 'U') is not null drop table assignment_x_account;

create table assignment_x_account (
	assignment_id uniqueidentifier not null,	
	account_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl preparation_geometry */

if OBJECT_ID('dbo.preparation_geometry', 'U') is not null drop table preparation_geometry;

create table preparation_geometry (
	id uniqueidentifier primary key not null,
	name nvarchar(80) not null,
	min_fill_height_mm float default 0,
	max_fill_height_mm float default 0,	
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_preparation_geometry
	@id uniqueidentifier,
	@name nvarchar(80),
	@min_fill_height float,
	@max_fill_height float,
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier	
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
		@update_id
	);
go

create proc csp_update_preparation_geometry
	@id uniqueidentifier,
	@name nvarchar(80),
	@min_fill_height float,
	@max_fill_height float,
	@instance_status_id int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	update preparation_geometry set 
		name = @name,
		min_fill_height_mm = @min_fill_height,
		max_fill_height_mm = @max_fill_height,
		instance_status_id = @instance_status_id,
		comment = @comment,		
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_preparation_geometry
	@id uniqueidentifier
as 
	select * from preparation_geometry where id = @id
go

create proc csp_select_preparation_geometries
	@instance_status_level int
as
	select *
	from preparation_geometry
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_preparation_geometries_short
	@instance_status_level int
as
	select
		id,
		name
	from preparation_geometry
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_preparation_geometries_flat
	@instance_status_level int
as
	select 
		pb.id,
		pb.name,
		pb.min_fill_height_mm, 
		pb.max_fill_height_mm, 
		st.name as 'instance_status_name',
		pb.comment,
		pb.create_date,
		pb.create_id,
		pb.update_date,
		pb.update_id,
		pb.volume_l,
		pb.radius_mm
	from preparation_geometry pb, instance_status st
	where pb.instance_status_id = st.id and pb.instance_status_id <= @instance_status_level
	order by pb.name
go

/*===========================================================================*/
/* tbl preparation_method */

if OBJECT_ID('dbo.preparation_method', 'U') is not null drop table preparation_method;

create table preparation_method (
	id uniqueidentifier primary key not null,
	name nvarchar(200) not null,
	name_short nvarchar(20) not null,
	description_link nvarchar(1024) default null,
	destructive bit default 0,	
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_preparation_method
	@id uniqueidentifier,
	@name nvarchar(200),
	@name_short nvarchar(20),
	@description_link nvarchar(256),	
	@destructive bit,	
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	insert into preparation_method values (
		@id,
		@name,
		@name_short,
		@description_link,		
		@destructive,
		@instance_status_id,
		@comment,		
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_preparation_method
	@id uniqueidentifier,
	@name nvarchar(200),
	@name_short nvarchar(20),
	@description_link nvarchar(256),	
	@destructive bit,	
	@instance_status_id int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	update preparation_method set 
		name = @name,
		name_short = @name_short,
		description_link = @description_link,
		destructive = @destructive,
		instance_status_id = @instance_status_id,
		comment = @comment,		
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_preparation_method
	@id uniqueidentifier
as 
	select * from preparation_method where id = @id
go

create proc csp_select_preparation_methods_for_sample_type_short
	@sample_type_id uniqueidentifier
as 
	select 
		pm.id, 
		pm.name 
	from preparation_method pm	
		inner join sample_type_x_preparation_method stpm on stpm.preparation_method_id = pm.id
		inner join sample_type st on stpm.sample_type_id = st.id and st.id = @sample_type_id
	order by name
go

create proc csp_select_preparation_methods
	@instance_status_level int
as
	select *
	from preparation_method
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_preparation_methods_short
	@instance_status_level int
as
	select 
		id as 'id', 
		name_short as 'name'
	from preparation_method
	where instance_status_id <= @instance_status_level
	order by name_short
go

create proc csp_select_preparation_methods_for_laboratory_sample_type_short
	@laboratory_id uniqueidentifier,
	@sample_type_id uniqueidentifier,
	@instance_status_level int
as
	select 
		pm.id as 'id', 
		pm.name_short as 'name'
	from preparation_method pm
		inner join laboratory_x_preparation_method lxpm on lxpm.preparation_method_id = pm.id and lxpm.laboratory_id = @laboratory_id
		inner join sample_type_x_preparation_method stxpm on pm.id = stxpm.preparation_method_id and stxpm.sample_type_id = @sample_type_id
	where instance_status_id <= @instance_status_level
	order by name_short
go

create proc csp_select_preparation_methods_flat
	@instance_status_level int
as
	select 
		pm.id,
		pm.name,
		pm.name_short,
		pm.description_link, 
		pm.destructive, 
		st.name as 'instance_status_name',
		pm.comment,
		pm.create_date,
		pm.create_id,
		pm.update_date,
		pm.update_id
	from preparation_method pm, instance_status st
	where pm.instance_status_id = st.id and pm.instance_status_id <= @instance_status_level
	order by pm.name
go

create proc csp_select_preparation_methods_for_sample_type
	@sample_type_id uniqueidentifier
as
	select pm.* 
	from preparation_method pm, sample_type_x_preparation_method stpm
	where pm.id = stpm.preparation_method_id and stpm.sample_type_id = @sample_type_id
	order by pm.name
go

/*===========================================================================*/
/* tbl laboratory_x_preparation_method */

if OBJECT_ID('dbo.laboratory_x_preparation_method', 'U') is not null drop table laboratory_x_preparation_method;

create table laboratory_x_preparation_method (
	laboratory_id uniqueidentifier not null,
	preparation_method_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl preparation */

if OBJECT_ID('dbo.preparation', 'U') is not null drop table preparation;

create table preparation (
	id uniqueidentifier primary key not null,
	sample_id uniqueidentifier not null,
	number int not null,
	assignment_id uniqueidentifier default null,
	laboratory_id uniqueidentifier not null,
	preparation_geometry_id uniqueidentifier default null,
	preparation_method_id uniqueidentifier not null,
	workflow_status_id int not null default 1,
	amount float default 0,
	prep_unit_id int not null default 0,		
	quantity float default 0,
	quantity_unit_id int not null default 0,
	fill_height_mm float default 0,		
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,	
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_preparation
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
	@update_id uniqueidentifier
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
		@update_id
	);
go

create proc csp_update_preparation
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
	@update_id uniqueidentifier
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
		update_id = @update_id
	where id = @id
go

create proc csp_select_preparation
	@id uniqueidentifier
as
	select * from preparation where id = @id
go

create proc csp_select_preparation_headers_for_sample
	@sample_id uniqueidentifier
as
	select 
		p.id as 'preparation_id', 
		p.number as 'preparation_number', 
		a.name as 'assignment_name', 
		pm.name_short as 'preparation_method_name', 
		pm.name as 'preparation_method_name_full', 
		p.workflow_status_id
	from preparation p 
		inner join preparation_method pm on pm.id = p.preparation_method_id
		left outer join assignment a on a.id = p.assignment_id
	where sample_id = @sample_id and p.instance_status_id = 1
	order by p.number
go

create proc csp_select_preparation_headers_for_sample_assignment
	@sample_id uniqueidentifier,
	@assignment_id uniqueidentifier
as
	select
		p.id as 'preparation_id',
		p.number as 'preparation_number',
		pm.name as 'preparation_method_name',
		ws.id as 'workflow_status_id',
		ws.name as 'workflow_status_name',
		p.comment as 'preparation_comment'
	from preparation p
		inner join preparation_method pm on p.preparation_method_id = pm.id
		inner join workflow_status ws on p.workflow_status_id = ws.id
	where p.sample_id = @sample_id and p.assignment_id = @assignment_id and p.instance_status_id = 1
	order by p.number
go

create proc csp_select_preparation_flat
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

create proc csp_select_preparation_informative
	@id uniqueidentifier
as
	select 
	p.id,
	p.number as 'preparation_number',
	s.number as 'sample_number',
	ass.name as 'assignment',
	l.name as 'laboratory',
	pg.name as 'geometry',
	pm.name as 'preparation_method',
	ws.name as 'workflow_status',
	p.amount,
	pu.name as 'amount_unit',
	p.quantity,
	qu.name as 'quantity_unit',
	p.fill_height_mm,
	inst.name as 'instance_status',
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

/*===========================================================================*/
/* tbl analysis_method */

if OBJECT_ID('dbo.analysis_method', 'U') is not null drop table analysis_method;

create table analysis_method (
	id uniqueidentifier primary key not null,
	name nvarchar(200) not null,
	name_short nvarchar(20) not null,
	description_link nvarchar(1024) default null,
	specter_reference_regexp nvarchar(256) default null,	
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_analysis_method
	@id uniqueidentifier,
	@name nvarchar(200),
	@name_short nvarchar(20),
	@description_link nvarchar(256),	
	@specter_reference_regexp nvarchar(256),	
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	insert into analysis_method values (
		@id,
		@name,
		@name_short,
		@description_link,		
		@specter_reference_regexp,
		@instance_status_id,
		@comment,		
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_analysis_method
	@id uniqueidentifier,
	@name nvarchar(200),
	@name_short nvarchar(20),
	@description_link nvarchar(256),	
	@specter_reference_regexp nvarchar(256),	
	@instance_status_id int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	update analysis_method set 
		name = @name,
		name_short = @name_short,
		description_link = @description_link,
		specter_reference_regexp = @specter_reference_regexp,
		instance_status_id = @instance_status_id,
		comment = @comment,		
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_analysis_method
	@id uniqueidentifier
as 
	select * from analysis_method where id = @id
go

create proc csp_select_analysis_methods
	@instance_status_level int
as
	select *
	from analysis_method
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_analysis_methods_short
	@instance_status_level int
as
	select 
		id as 'id', 
		name_short as 'name'
	from analysis_method
	where instance_status_id <= @instance_status_level
	order by name_short
go

create proc csp_select_analysis_methods_for_laboratory_short
	@laboratory_id uniqueidentifier,
	@preparation_method_id uniqueidentifier,
	@instance_status_level int
as
	select 
		am.id as 'id', 
		am.name_short as 'name'
	from analysis_method am
		inner join laboratory_x_analysis_method lxam on lxam.analysis_method_id = am.id and lxam.laboratory_id = @laboratory_id
		inner join preparation_method_x_analysis_method pmxam on pmxam.analysis_method_id = am.id and pmxam.preparation_method_id = @preparation_method_id
	where instance_status_id <= @instance_status_level
	order by am.name_short
go

create proc csp_select_analysis_methods_for_laboratory_and_preparation_method_short
	@laboratory_id uniqueidentifier,
	@preparation_method_id uniqueidentifier,
	@instance_status_level int
as
	select 
		am.id as 'id', 
		am.name_short as 'name'
	from analysis_method am
		inner join laboratory_x_analysis_method lxam on lxam.analysis_method_id = am.id and lxam.laboratory_id = @laboratory_id 
		inner join preparation_method_x_analysis_method pmxam on pmxam.preparation_method_id = @preparation_method_id and pmxam.analysis_method_id = am.id
	where instance_status_id <= @instance_status_level
	order by am.name_short
go

create proc csp_select_analysis_methods_flat
	@instance_status_level int
as
	select 
		am.id,
		am.name,
		am.name_short,
		am.description_link, 
		am.specter_reference_regexp, 
		st.name as 'instance_status_name',
		am.comment,
		am.create_date,
		am.create_id,
		am.update_date,
		am.update_id
	from analysis_method am, instance_status st
	where am.instance_status_id = st.id and am.instance_status_id <= @instance_status_level
	order by am.name
go

/*===========================================================================*/
/* tbl laboratory_x_analysis_method */

if OBJECT_ID('dbo.laboratory_x_analysis_method', 'U') is not null drop table laboratory_x_analysis_method;

create table laboratory_x_analysis_method (
	laboratory_id uniqueidentifier not null,
	analysis_method_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl account_x_analysis_method */

if OBJECT_ID('dbo.account_x_analysis_method', 'U') is not null drop table account_x_analysis_method;

create table account_x_analysis_method (
	account_id uniqueidentifier not null,
	analysis_method_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl analysis */

if OBJECT_ID('dbo.analysis', 'U') is not null drop table analysis;

create table analysis (
	id uniqueidentifier primary key not null,
	number int not null,
	assignment_id uniqueidentifier default null,
	laboratory_id uniqueidentifier not null,
	preparation_id uniqueidentifier not null,
	analysis_method_id uniqueidentifier not null,	
	workflow_status_id int not null default 1,
	specter_reference nvarchar(256) default null,
	activity_unit_id uniqueidentifier default null,
	activity_unit_type_id uniqueidentifier default null,
	sigma_act float not null default 2.0,
	sigma_mda float not null default 1.645,
	nuclide_library nvarchar(256) default null,
	mda_library nvarchar(256) default null,	
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,	
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_analysis
	@id uniqueidentifier,
	@number int,
	@assignment_id uniqueidentifier,
	@laboratory_id uniqueidentifier,
	@preparation_id uniqueidentifier,
	@analysis_method_id uniqueidentifier,	
	@workflow_status_id int,
	@specter_reference nvarchar(256),
	@activity_unit_id uniqueidentifier,
	@activity_unit_type_id uniqueidentifier,
	@sigma_act float,
	@sigma_mda float,
	@nuclide_library nvarchar(256),
	@mda_library nvarchar(256),	
	@instance_status_id int,
	@comment nvarchar(1000),	
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as 
	insert into analysis values (
		@id,
		@number,
		@assignment_id,
		@laboratory_id,
		@preparation_id,
		@analysis_method_id,	
		@workflow_status_id,
		@specter_reference,
		@activity_unit_id,
		@activity_unit_type_id,
		@sigma_act,
		@sigma_mda,
		@nuclide_library,
		@mda_library,	
		@instance_status_id,
		@comment,	
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_analysis
	@id uniqueidentifier,
	@workflow_status_id int,
	@specter_reference nvarchar(256),
	@activity_unit_id uniqueidentifier,
	@activity_unit_type_id uniqueidentifier,
	@sigma_act float,
	@sigma_mda float,
	@nuclide_library nvarchar(256),
	@mda_library nvarchar(256),		
	@comment nvarchar(1000),		
	@update_date datetime,
	@update_id uniqueidentifier
as 
	update analysis set
		workflow_status_id = @workflow_status_id,
		specter_reference = @specter_reference,
		activity_unit_id = @activity_unit_id,
		activity_unit_type_id = @activity_unit_type_id,
		sigma_act = @sigma_act,
		sigma_mda = @sigma_mda,
		nuclide_library = @nuclide_library,
		mda_library = @mda_library,
		comment = @comment,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_analysis
	@id uniqueidentifier
as
	select * from analysis where id = @id
go

create proc csp_select_analysis_headers_for_preparation
	@preparation_id uniqueidentifier
as
	select 
		a.id as 'analysis_id', 
		a.number as 'analysis_number', 
		am.name_short as 'analysis_method_name', 
		am.name as 'analysis_method_name_full', 
		ass.name as 'assignment_name', 
		a.workflow_status_id
	from analysis a 
		inner join analysis_method am on am.id = a.analysis_method_id
		left outer join assignment ass on ass.id = a.assignment_id
	where a.preparation_id = @preparation_id and a.instance_status_id = 1
	order by a.number
go

create proc csp_select_analysis_headers_for_preparation_assignment
	@preparation_id uniqueidentifier,
	@assignment_id uniqueidentifier
as
	select 
		a.id as 'analysis_id',
		a.number as 'analysis_number',
		am.name as 'analysis_method_name',
		ws.id as 'workflow_status_id',
		ws.name as 'workflow_status_name',
		a.comment as 'analysis_comment'
	from analysis a
		inner join analysis_method am on a.analysis_method_id = am.id
		inner join workflow_status ws on a.workflow_status_id = ws.id
	where a.preparation_id = @preparation_id and a.assignment_id = @assignment_id and a.instance_status_id = 1
	order by a.number
go

create proc csp_select_analysis_flat
	@id uniqueidentifier
as
	select
		a.id as 'id',
		a.number as 'analysis_number',
		ass.name as 'assignment_name',
		l.name as 'laboratory_name',
		p.number as 'preparation_number',
		am.name as 'analysis_method_name',
		ws.name as 'workflow_status_name',
		a.specter_reference as 'specter_reference',
		au.name as 'activity_unit_name',
		aut.name as 'activity_unit_type_name',
		a.sigma_act,
		a.sigma_mda,
		a.nuclide_library,
		a.mda_library,
		inst.name as 'instance_status_name',
		a.comment,
		a.create_date,
		a.create_id,
		a.update_date,
		a.update_id
	from analysis a
		left outer join assignment ass on ass.id = a.assignment_id
		left outer join laboratory l on l.id = a.laboratory_id
		left outer join preparation p on p.id = a.preparation_id
		left outer join analysis_method am on am.id = a.analysis_method_id
		left outer join workflow_status ws on ws.id = a.workflow_status_id
		left outer join activity_unit au on au.id = a.activity_unit_id
		left outer join activity_unit_type aut on aut.id = a.activity_unit_type_id
		left outer join instance_status inst on inst.id = a.instance_status_id
	where a.id = @id
go

create proc csp_select_analysis_informative
	@id uniqueidentifier
as
	select
	a.id as 'id',
	a.number as 'analysis_number',
	ass.name as 'assignment',
	l.name as 'laboratory',
	p.number as 'preparation_number',
	am.name as 'analysis_method',
	ws.name as 'workflow_status',
	a.specter_reference as 'specter_reference',
	au.name as 'activity_unit',
	aut.name as 'activity_unit_type',
	a.sigma_act,
	a.sigma_mda,
	a.nuclide_library,
	a.mda_library,
	inst.name as 'instance_status',
	a.comment,
	a.create_date,
	a.create_id,
	a.update_date,
	a.update_id
from analysis a
	left outer join assignment ass on ass.id = a.assignment_id
	left outer join laboratory l on l.id = a.laboratory_id
	left outer join preparation p on p.id = a.preparation_id
	left outer join analysis_method am on am.id = a.analysis_method_id
	left outer join workflow_status ws on ws.id = a.workflow_status_id
	left outer join activity_unit au on au.id = a.activity_unit_id
	left outer join activity_unit_type aut on aut.id = a.activity_unit_type_id
	left outer join instance_status inst on inst.id = a.instance_status_id
where a.id = @id
go

/*===========================================================================*/
/* tbl nuclide */

if OBJECT_ID('dbo.nuclide', 'U') is not null drop table nuclide;

create table nuclide (
	id uniqueidentifier primary key not null,
	zas int not null,
	name nvarchar(32) unique not null,
	protons int not null,
	neutrons int not null,
	meta_stable bit not null,
	half_life_year float not null,
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_nuclide
	@id uniqueidentifier,
	@zas int,	
	@name nvarchar(32),	
	@protons int,
	@neutrons int,
	@meta_stable bit,
	@half_life_year float,
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as 
	insert into nuclide values (
		@id, 
		@zas,
		@name, 
		@protons,
		@neutrons,
		@meta_stable,
		@half_life_year, 	
		@instance_status_id,
		@comment,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_nuclide
	@id uniqueidentifier,
	@zas int,	
	@name nvarchar(32),	
	@protons int,
	@neutrons int,
	@meta_stable bit,
	@half_life_year float,	
	@instance_status_id int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@update_id uniqueidentifier
as 
	update nuclide set
		zas = @zas,
		name = @name, 				
		protons = @protons,
		neutrons = @neutrons,
		meta_stable = @meta_stable,
		half_life_year = @half_life_year, 	
		instance_status_id = @instance_status_id,
		comment = @comment,		
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_nuclides
	@instance_status_level int
as 
	select * 
	from nuclide 
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_nuclides_short
	@instance_status_level int
as 
	select id, name
	from nuclide 
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_nuclides_flat
	@instance_status_level int
as
	select 
		n.id,
		n.zas,	
		n.name,	
		n.protons,
		n.neutrons,
		n.meta_stable,
		n.half_life_year,
		st.name as 'instance_status_name',
		n.comment,
		n.create_date,
		n.create_id,
		n.update_date,
		n.update_id
	from nuclide n, instance_status st
	where n.instance_status_id = st.id and n.instance_status_id <= @instance_status_level
	order by n.name
go

create proc csp_select_nuclide
	@id uniqueidentifier
as 
	select * from nuclide where id = @id
go

create proc csp_select_nuclides_for_analysis_method
	@analysis_method_id uniqueidentifier
as 
	select n.id, n.name 
	from nuclide n inner join analysis_method_x_nuclide amn on amn.nuclide_id = n.id 
		and amn.analysis_method_id = @analysis_method_id    
	order by name
go

/*===========================================================================*/
/* tbl project_main */

if OBJECT_ID('dbo.project_main', 'U') is not null drop table project_main;

create table project_main (
	id uniqueidentifier primary key not null,
	name nvarchar(256) not null,
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,		
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_project_main
	@id uniqueidentifier,
	@name nvarchar(256),	
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as 
	insert into project_main values (
		@id,
		@name, 				
		@instance_status_id,
		@comment,		
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_project_main
	@id uniqueidentifier,
	@name nvarchar(256),	
	@instance_status_id int,
	@comment nvarchar(1000),
	@update_date datetime,
	@update_id uniqueidentifier
as 
	update project_main set
		name = @name, 				
		instance_status_id = @instance_status_id,
		comment = @comment,				
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_project_main
	@id uniqueidentifier	
as 
	select * from project_main where id = @id
go

create proc csp_select_projects_main
	@instance_status_level int
as 
	select * 
	from project_main
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_projects_main_flat
	@instance_status_level int
as 
	select
		pm.id,
		pm.name,
		st.name as 'instance_status_name',
		pm.comment,
		pm.create_date,
		pm.create_id,
		pm.update_date,
		pm.update_id
	from project_main pm, instance_status st
	where pm.instance_status_id = st.id and pm.instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_projects_main_short
	@instance_status_level int
as 
	select 
		id, 
		name	
	from project_main 
	where instance_status_id <= @instance_status_level
	order by instance_status_id, name
go

/*===========================================================================*/
/* tbl project_sub */

if OBJECT_ID('dbo.project_sub', 'U') is not null drop table project_sub;

create table project_sub (
	id uniqueidentifier primary key not null,
	project_main_id uniqueidentifier not null,
	name nvarchar(256) not null,
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,		
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_project_sub
	@id uniqueidentifier,
	@project_main_id uniqueidentifier,
	@name nvarchar(256),	
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as 
	insert into project_sub values (
		@id,
		@project_main_id,
		@name, 				
		@instance_status_id,
		@comment,		
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_project_sub
	@id uniqueidentifier,
	@project_main_id uniqueidentifier,
	@name nvarchar(256),	
	@instance_status_id int,
	@comment nvarchar(1000),
	@update_date datetime,
	@update_id uniqueidentifier
as 
	update project_sub set
		name = @name,
		project_main_id = @project_main_id,
		instance_status_id = @instance_status_id,
		comment = @comment,				
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_project_sub
	@id uniqueidentifier	
as 
	select * from project_sub where id = @id
go

create proc csp_select_projects_sub
	@project_main_id uniqueidentifier,
	@instance_status_level int
as 
	select * 
	from project_sub 
	where project_main_id = @project_main_id and instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_projects_sub_flat
	@project_main_id uniqueidentifier,
	@instance_status_level int
as 
	select
		ps.id,
		pm.name as 'project_main_name',
		ps.name,
		st.name as 'instance_status_name',
		ps.comment,
		ps.create_date,
		ps.create_id,
		ps.update_date,
		ps.update_id
	from project_sub ps, project_main pm, instance_status st
	where ps.project_main_id = pm.id and ps.project_main_id = @project_main_id and ps.instance_status_id = st.id and ps.instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_projects_sub_short
	@project_main_id uniqueidentifier,
	@instance_status_level int
as 
	select 
		id, 
		name	
	from project_sub 
	where project_main_id = @project_main_id and instance_status_id <= @instance_status_level
	order by instance_status_id, name
go

/*===========================================================================*/
/* tbl project_sub_x_account */

if OBJECT_ID('dbo.project_sub_x_account', 'U') is not null drop table project_sub_x_account;

create table project_sub_x_account (
	project_sub_id uniqueidentifier not null,
	account_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl station */

if OBJECT_ID('dbo.station', 'U') is not null drop table station;

create table station (
	id uniqueidentifier primary key not null,
	name nvarchar(128) unique not null,
	latitude float default 0,
	longitude float default 0,
	altitude float default 0,	
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
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
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier	
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
		@create_id,
		@update_date,
		@update_id
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
	@update_id uniqueidentifier	
as 
	update station set 
		name = @name,
		latitude = @latitude,	
		longitude = @longitude,	
		altitude = @altitude,	
		instance_status_id = @instance_status_id,
		comment = @comment,	
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_station
	@id uniqueidentifier
as 
	select * from station where id = @id
go

create proc csp_select_stations
	@instance_status_level int
as
	select *
	from station
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_stations_short
	@instance_status_level int
as
	select 
		id, 
		name
	from station
	where instance_status_id <= @instance_status_level
	order by instance_status_id, name
go

create proc csp_select_stations_flat
	@instance_status_level int
as
	select 
		s.id,
		s.name,
		s.latitude, 	
		s.longitude, 	
		s.altitude, 	
		st.name as 'instance_status_name',	
		s.comment,	
		s.create_date,
		s.create_id,
		s.update_date,
		s.update_id
	from station s, instance_status st
	where s.instance_status_id = st.id and s.instance_status_id <= @instance_status_level
	order by s.name
go

/*===========================================================================*/
/* tbl sample_type */

if OBJECT_ID('dbo.sample_type', 'U') is not null drop table sample_type;

create table sample_type (
	id uniqueidentifier primary key not null,
	parent_id uniqueidentifier default null,	
	path nvarchar(2000) not null,
	name nvarchar(80) not null,
	name_common nvarchar(80) default null,
	name_latin nvarchar(80) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_sample_type
	@id uniqueidentifier,	
	@parent_id uniqueidentifier,		
	@path nvarchar(2000),
	@name nvarchar(80),
	@name_common nvarchar(80),
	@name_latin nvarchar(80),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	insert into sample_type values (
		@id,
		@parent_id,
		@path,
		@name,
		@name_common,
		@name_latin,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_sample_type
	@id uniqueidentifier,		
	@path nvarchar(2000),
	@name nvarchar(80),
	@name_common nvarchar(80),
	@name_latin nvarchar(80),	
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	update sample_type set 
		path = @path,
		name = @name,
		name_common = @name_common,
		name_latin = @name_latin,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_sample_type
	@id uniqueidentifier
as
	select *
	from sample_type
	where id = @id
go

create proc csp_select_sample_types
as
	select *
	from sample_type
	order by name
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

if OBJECT_ID('dbo.sample_storage', 'U') is not null drop table sample_storage;

create table sample_storage (
	id uniqueidentifier primary key not null,
	name nvarchar(256) unique not null,
	address nvarchar(1000) default null,	
	instance_status_id int not null default 1,
	comment nvarchar(1000) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_sample_storage
	@id uniqueidentifier,	
	@name nvarchar(80),
	@address nvarchar(1000),
	@instance_status_id int,
	@comment nvarchar(1000),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	insert into sample_storage values (
		@id,		
		@name,
		@address,				
		@instance_status_id,
		@comment,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_sample_storage
	@id uniqueidentifier,	
	@name nvarchar(80),
	@address nvarchar(1000),		
	@instance_status_id int,
	@comment nvarchar(1000),	
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	update sample_storage set 
		name = @name,
		address = @address,			
		instance_status_id = @instance_status_id,
		comment = @comment,	
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_sample_storage
	@id uniqueidentifier
as 
	select * from sample_storage where id = @id
go

create proc csp_select_sample_storages
	@instance_status_level int
as
	select * 
	from sample_storage 
	where instance_status_id <= @instance_status_level
	order by name
go

create proc csp_select_sample_storages_short
	@instance_status_level int
as
	select id, name
	from sample_storage 
	where instance_status_id <= @instance_status_level
	order by instance_status_id, name
go

create proc csp_select_sample_storages_flat
	@instance_status_level int
as
	select 
		s.id,
		s.name,
		s.address,
		st.name as 'instance_status_name',
		s.comment,	
		s.create_date,
		s.create_id,
		s.update_date,
		s.update_id
	from sample_storage s, instance_status st
	where s.instance_status_id = st.id and s.instance_status_id <= @instance_status_level
	order by s.name
go

/*===========================================================================*/
/* tbl sample_component */

if OBJECT_ID('dbo.sample_component', 'U') is not null drop table sample_component;

create table sample_component (
	id uniqueidentifier primary key not null,
	sample_type_id uniqueidentifier not null,
	name nvarchar(80) not null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_sample_component
	@id uniqueidentifier,	
	@sample_type_id uniqueidentifier,
	@name nvarchar(80),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	insert into sample_component values (
		@id,		
		@sample_type_id,
		@name,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_sample_component
	@id uniqueidentifier,
	@name nvarchar(80),
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	update sample_component set 
		name = @name,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_sample_component
	@id uniqueidentifier
as
	select *
	from sample_component
	where id = @id
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
/* tbl sample_parameter_type */

if OBJECT_ID('dbo.sample_parameter_type', 'U') is not null drop table sample_parameter_type;

create table sample_parameter_type (
	name nvarchar(80) primary key not null	
)
go

insert into sample_parameter_type values('Integer')
insert into sample_parameter_type values('Decimal')
insert into sample_parameter_type values('Text')

/*===========================================================================*/
/* tbl sample_parameter_name */

if OBJECT_ID('dbo.sample_parameter_name', 'U') is not null drop table sample_parameter_name;

create table sample_parameter_name (
	id uniqueidentifier primary key not null,	
	name nvarchar(80) not null,
	type nvarchar(80) not null	
)
go

/*===========================================================================*/
/* tbl sample_parameter */

if OBJECT_ID('dbo.sample_parameter', 'U') is not null drop table sample_parameter;

create table sample_parameter (
	id uniqueidentifier primary key not null,
	sample_id uniqueidentifier not null,	
	sample_parameter_name_id uniqueidentifier not null,
	value nvarchar(80) default null,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_insert_sample_parameter
	@id uniqueidentifier,	
	@sample_id uniqueidentifier,
	@sample_parameter_name_id uniqueidentifier,
	@value nvarchar(80),
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	insert into sample_parameter values (
		@id,		
		@sample_id,
		@sample_parameter_name_id,
		@value,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_sample_parameter
	@id uniqueidentifier,
	@value nvarchar(80),
	@update_date datetime,
	@update_id uniqueidentifier	
as 
	update sample_parameter set 
		value = @value,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_sample_parameter
	@id uniqueidentifier
as
	select *
	from sample_parameter
	where id = @id	
go

create proc csp_select_sample_parameter_flat
	@id uniqueidentifier
as
	select sp.*, spn.name, spn.type
	from sample_parameter sp
		inner join sample_parameter_name spn on spn.id = sp.sample_parameter_name_id
	where sp.id = @id	
go

create proc csp_select_sample_parameters
	@sample_id uniqueidentifier
as
	select *
	from sample_parameter
	where sample_id = @sample_id		
go

create proc csp_select_sample_parameters_flat
	@sample_id uniqueidentifier
as
	select sp.id, spn.name, spn.type, sp.value
	from sample_parameter sp
		inner join sample_parameter_name spn on spn.id = sp.sample_parameter_name_id
	where sp.sample_id = @sample_id		
	order by spn.name
go

/*===========================================================================*/
/* tbl sample */

if OBJECT_ID('dbo.sample', 'U') is not null drop table sample;

create table sample (
	id uniqueidentifier primary key not null,
	number int unique not null,	
	laboratory_id uniqueidentifier not null,	
	sample_type_id uniqueidentifier not null,	
	sample_storage_id uniqueidentifier default null,
	sample_component_id uniqueidentifier default null,
	project_sub_id uniqueidentifier not null,
	station_id uniqueidentifier default null,
	sampler_id uniqueidentifier default null,
	sampling_method_id uniqueidentifier default null,
	transform_from_id uniqueidentifier default null,	
	transform_to_id uniqueidentifier default null,		
	imported_from nvarchar(128) default null,
	imported_from_id nvarchar(128) default null,		
	municipality_id uniqueidentifier default null,
	location_type nvarchar(50) default null,
	location nvarchar(128) default null,		
	latitude float default 0,
	longitude float default 0,
	altitude float default 0,
	sampling_date_from datetime default null,	
	sampling_date_to datetime default null,
	reference_date datetime default null,
	external_id nvarchar(128) default null,
	wet_weight_g float default null,	
	dry_weight_g float default null,
	volume_l float default null,
	lod_weight_start float default null,	
	lod_weight_end float default null,	
	lod_temperature float default null,
	confidential bit default 0,
	instance_status_id int not null default 1,
	locked_id uniqueidentifier default null,
	comment nvarchar(1000) default null,	
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null
)
go

create proc csp_increment_sample_counter
	@current_count int output
as
	select @current_count = value from counters where name = 'sample_counter';
	update counters set value = @current_count + 1 where name = 'sample_counter';
	return
go

create proc csp_insert_sample
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
		@update_id
	);
go

create proc csp_update_sample
	@id uniqueidentifier,	
	@laboratory_id uniqueidentifier,	
	@sample_type_id uniqueidentifier,	
	@sample_storage_id uniqueidentifier,
	@sample_component_id uniqueidentifier,
	@project_sub_id uniqueidentifier,
	@station_id uniqueidentifier,
	@sampler_id uniqueidentifier,
	@sampling_method_id uniqueidentifier,	
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
	@confidential bit,		
	@instance_status_id int,	
	@comment nvarchar(1000),
	@update_date datetime,
	@update_id uniqueidentifier	
as 		
	update sample set		
		laboratory_id = @laboratory_id,	
		sample_type_id = @sample_type_id,	
		sample_storage_id = @sample_storage_id,
		sample_component_id = @sample_component_id,
		project_sub_id = @project_sub_id,
		station_id = @station_id,
		sampler_id = @sampler_id,
		sampling_method_id = @sampling_method_id,	
		municipality_id = @municipality_id,
		location_type = @location_type,
		location = @location,	
		latitude = @latitude,
		longitude = @longitude,
		altitude = @altitude,
		sampling_date_from = @sampling_date_from,		
		sampling_date_to = @sampling_date_to,
		reference_date = @reference_date,
		external_id = @external_id,	
		confidential = @confidential,			
		instance_status_id = @instance_status_id,	
		comment = @comment,
		update_date = @update_date,
		update_id = @update_id		
	where id = @id
go

create proc csp_update_sample_info
	@id uniqueidentifier,	
	@wet_weight_g float,	
	@dry_weight_g float,
	@volume_l float,
	@lod_weight_start float,	
	@lod_weight_end float,	
	@lod_temperature float,
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
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_sample
	@id uniqueidentifier
as
	select * 
	from sample 
	where id = @id
go

create proc csp_select_sample_flat
	@id uniqueidentifier
as
	select
		s.id,	
		s.number,
		l.name as 'laboratory_name',	
		st.name as 'sample_type_name',	
		ss.name as 'sample_storage_name',
		sc.name as 'sample_component_name',
		pm.name as 'project_main_name',
		ps.name as 'project_sub_name',
		sta.name as 'station_name',
		sa.person_name as 'sampler_name',
		sm.name as 'sampling_method_name',
		(select number from sample where id = s.transform_from_id) as 'split_parent',	
		(select number from sample where id = s.transform_to_id) as 'merge_child',
		s.imported_from,
		s.imported_from_id,		
		co.name as 'county_name',
		mun.name as 'municipality_name',
		s.location_type,
		s.location,	
		s.latitude,
		s.longitude,
		s.altitude,
		s.sampling_date_from,		
		s.sampling_date_to,
		s.reference_date,
		s.external_id,
		s.wet_weight_g,	
		s.dry_weight_g,
		s.volume_l,
		s.lod_weight_start,	
		s.lod_weight_end,	
		s.lod_temperature,
		s.confidential,			
		insta.name as 'instance_status_name',
		(select name from cv_account where id = s.locked_id) as 'locked_name',
		s.comment,	
		s.create_date,
		s.create_id,
		s.update_date,
		s.update_id
	from sample s 
		left outer join laboratory l on s.laboratory_id = l.id
		left outer join sample_type st on s.sample_type_id = st.id
		left outer join sample_storage ss on s.sample_storage_id = ss.id
		left outer join sample_component sc on s.sample_component_id = sc.id
		inner join project_sub ps on s.project_sub_id = ps.id
		inner join project_main pm on pm.id = ps.project_main_id
		left outer join station sta on s.station_id = sta.id
		left outer join cv_sampler sa on s.sampler_id = sa.id
		left outer join sampling_method sm on s.sampling_method_id = sm.id
		left outer join municipality mun on s.municipality_id = mun.id
		left outer join county co on mun.county_id = co.id
		inner join instance_status insta on s.instance_status_id = insta.id
	where s.id = @id
go

create proc csp_select_sample_info
	@id uniqueidentifier	
as 		
	select
		sc.name as 'sample_component_name',
		s.external_id,
		pm.name + ' - ' + ps.name as 'project_name',
		s.reference_date,
		ss.name as 'sample_storage_name',
		s.wet_weight_g,
		s.dry_weight_g,
		s.volume_l,
		s.lod_weight_start,
		s.lod_weight_end,
		s.lod_temperature,
		s.comment
	from sample s		
		inner join project_sub ps on s.project_sub_id = ps.id
		inner join project_main pm on pm.id = ps.project_main_id
		left outer join sample_component sc on s.sample_component_id = sc.id
		left outer join sample_storage ss on s.sample_storage_id = ss.id
	where s.id = @id
go

create proc csp_select_sample_header
	@id uniqueidentifier	
as 		
	select 
		s.id as 'sample_id', 
		s.number as 'sample_number', 
		st.name as 'sample_type_name', 
		l.name as 'laboratory_name'
	from sample s
		inner join sample_type st on st.id = s.sample_type_id
		inner join laboratory l on l.id = s.laboratory_id
	where s.id = @id
go

create proc csp_select_sample_headers_for_assignment
	@assignment_id uniqueidentifier	
as 		
	select 
		s.id as 'sample_id',
		s.number as 'sample_number',	
		st.name as 'sample_type_name',
		sc.name as 'sample_component_name',
		s.comment as 'sample_comment'
	from sample s
		inner join sample_x_assignment_sample_type sxast on sxast.sample_id = s.id
		inner join assignment_sample_type ast on sxast.assignment_sample_type_id = ast.id
		inner join assignment ass on ast.assignment_id = ass.id
		inner join sample_type st on s.sample_type_id = st.id
		left outer join sample_component sc on s.sample_component_id = sc.id
	where ass.id = @assignment_id
	order by s.number
go

create proc csp_select_samples
	@instance_status_level int
as
	select * 
	from sample 
	where instance_status_id <= @instance_status_level
	order by create_date
go

create proc csp_select_samples_flat
	@instance_status_level int
as
	select
		s.id,	
		s.number,
		l.name as 'laboratory_name',	
		st.name as 'sample_type_name',	
		ss.name as 'sample_storage_name',
		sc.name as 'sample_component_name',
		pm.name as 'project_main_name',
		ps.name as 'project_sub_name',
		sta.name as 'station_name',
		sa.person_name as 'sampler_name',
		sm.name as 'sampling_method_name',
		(select number from sample where id = s.transform_from_id) as 'split_parent',	
		(select number from sample where id = s.transform_to_id) as 'merge_child',
		s.imported_from,
		s.imported_from_id,		
		co.name as 'county_name',
		mun.name as 'municipality_name',
		s.location_type,
		s.location,	
		s.latitude,
		s.longitude,
		s.altitude,
		s.sampling_date_from,		
		s.sampling_date_to,
		s.reference_date,
		s.external_id,
		s.wet_weight_g,	
		s.dry_weight_g,
		s.volume_l,
		s.lod_weight_start,	
		s.lod_weight_end,	
		s.lod_temperature,
		s.confidential,			
		insta.name as 'instance_status_name',
		(select name from cv_account where id = s.locked_id) as 'locked_name',
		s.comment,	
		s.create_date,
		s.create_id,
		s.update_date,
		s.update_id
	from sample s 
		left outer join laboratory l on s.laboratory_id = l.id
		left outer join sample_type st on s.sample_type_id = st.id
		left outer join sample_storage ss on s.sample_storage_id = ss.id
		left outer join sample_component sc on s.sample_component_id = sc.id
		inner join project_sub ps on s.project_sub_id = ps.id
		inner join project_main pm on pm.id = ps.project_main_id
		left outer join station sta on s.station_id = sta.id
		left outer join cv_sampler sa on s.sampler_id = sa.id
		left outer join sampling_method sm on s.sampling_method_id = sm.id
		left outer join municipality mun on s.municipality_id = mun.id
		left outer join county co on mun.county_id = co.id
		inner join instance_status insta on s.instance_status_id = insta.id
	where s.instance_status_id <= @instance_status_level
	order by s.create_date
go

create proc csp_select_samples_informative
	@instance_status_level int
as
	select
		s.id,	
		s.number,
		s.external_id,
		l.name as 'laboratory_name',
		st.name as 'sample_type_name',	
		sc.name as 'sample_component_name',
		pm.name + ' - ' + ps.name as 'project_name',
		ss.name as 'sample_storage_name',
		s.reference_date,
		insta.name as 'instance_status_name',
		(select name from cv_account where id = s.locked_id) as 'locked_name',
		(select number from sample where id = s.transform_from_id) as 'split_from',
		(select number from sample where id = s.transform_to_id) as 'merge_to',
		(select convert(varchar(80), number) + ', ' as 'data()' from sample where transform_to_id = s.id for XML PATH('')) as 'merge_from'
	from sample s 
		left outer join laboratory l on s.laboratory_id = l.id
		left outer join sample_type st on s.sample_type_id = st.id
		left outer join sample_storage ss on s.sample_storage_id = ss.id
		left outer join sample_component sc on s.sample_component_id = sc.id
		inner join project_sub ps on s.project_sub_id = ps.id
		inner join project_main pm on pm.id = ps.project_main_id
		inner join instance_status insta on s.instance_status_id = insta.id
	where s.instance_status_id <= @instance_status_level
	order by s.number desc
go

create proc csp_select_samples_for_assignment_flat
	@assignment_id uniqueidentifier
as
	select
		s.id,	
		s.number,
		s.external_id,
		l.name as 'laboratory_name',
		st.name as 'sample_type_name',	
		sc.name as 'sample_component_name',
		pm.name + ' - ' + ps.name as 'project_name'
from sample s inner join sample_x_assignment_sample_type sxast on s.id = sxast.sample_id
	inner join assignment_sample_type ast on sxast.assignment_sample_type_id = ast.id and ast.assignment_id = @assignment_id
	left outer join laboratory l on s.laboratory_id = l.id
    left outer join sample_type st on s.sample_type_id = st.id	
	left outer join sample_component sc on s.sample_component_id = sc.id
	inner join project_sub ps on s.project_sub_id = ps.id
	inner join project_main pm on pm.id = ps.project_main_id
go

/*===========================================================================*/
/* tbl sample_x_assignment_sample_type */

if OBJECT_ID('dbo.sample_x_assignment_sample_type', 'U') is not null drop table sample_x_assignment_sample_type;

create table sample_x_assignment_sample_type (
	sample_id uniqueidentifier not null,
	assignment_sample_type_id uniqueidentifier not null
)
go

create proc csp_insert_sample_x_assignment_sample_type
	@sample_id uniqueidentifier,	
	@assignment_sample_type_id uniqueidentifier
as 	
	insert into sample_x_assignment_sample_type 
	values(@sample_id, @assignment_sample_type_id)
go

/*===========================================================================*/
/* tbl sample_type_x_preparation_method */

if OBJECT_ID('dbo.sample_type_x_preparation_method', 'U') is not null drop table sample_type_x_preparation_method;

create table sample_type_x_preparation_method (
	sample_type_id uniqueidentifier not null,
	preparation_method_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl preparation_method_x_analysis_method */

if OBJECT_ID('dbo.preparation_method_x_analysis_method', 'U') is not null drop table preparation_method_x_analysis_method;

create table preparation_method_x_analysis_method (	
	preparation_method_id uniqueidentifier not null,
	analysis_method_id uniqueidentifier not null
)
go

create proc csp_select_analysis_methods_for_preparation_method
	@preparation_method_id uniqueidentifier
as
	select am.id, am.name from analysis_method am
		inner join preparation_method_x_analysis_method pmam on pmam.analysis_method_id = am.id 
			and pmam.preparation_method_id = @preparation_method_id
	order by name
go

/*===========================================================================*/
/* tbl analysis_method_x_nuclide */

if OBJECT_ID('dbo.analysis_method_x_nuclide', 'U') is not null drop table analysis_method_x_nuclide;

create table analysis_method_x_nuclide (		
	analysis_method_id uniqueidentifier not null,
	nuclide_id uniqueidentifier not null
)
go

/*===========================================================================*/
/* tbl analysis_result */

if OBJECT_ID('dbo.analysis_result', 'U') is not null drop table analysis_result;

create table analysis_result (
	id uniqueidentifier primary key not null,
	analysis_id uniqueidentifier not null,
	nuclide_id uniqueidentifier not null,
	activity float default null,	
	activity_uncertainty_abs float not null,
	activity_approved bit default 0,
	uniform_activity float default null,
	uniform_activity_unit_id int not null default 0,
	detection_limit float default null,
	detection_limit_approved bit default 0,
	accredited bit default 0,
	reportable bit default 0,
	instance_status_id int not null default 1,
	create_date datetime not null,
	create_id uniqueidentifier not null,
	update_date datetime not null,
	update_id uniqueidentifier not null	
)
go

create proc csp_insert_analysis_result
	@id uniqueidentifier,
	@analysis_id uniqueidentifier,
	@nuclide_id uniqueidentifier,
	@activity float,	
	@activity_uncertainty_abs float,
	@activity_approved bit,
	@uniform_activity float,
	@uniform_activity_unit_id int,
	@detection_limit float,
	@detection_limit_approved bit,
	@accredited bit,
	@reportable bit,
	@instance_status_id int,
	@create_date datetime,
	@create_id uniqueidentifier,
	@update_date datetime,
	@update_id uniqueidentifier
as 
	insert into analysis_result values (		
		@id,
		@analysis_id,
		@nuclide_id,
		@activity,	
		@activity_uncertainty_abs,
		@activity_approved,
		@uniform_activity,
		@uniform_activity_unit_id,
		@detection_limit,
		@detection_limit_approved,
		@accredited,
		@reportable,
		@instance_status_id,
		@create_date,
		@create_id,
		@update_date,
		@update_id
	);
go

create proc csp_update_analysis_result	
	@id uniqueidentifier,	
	@activity float,	
	@activity_uncertainty_abs float,
	@activity_approved bit,
	@uniform_activity float,
	@uniform_activity_unit_id int,
	@detection_limit float,
	@detection_limit_approved bit,
	@accredited bit,
	@reportable bit,
	@instance_status_id int,	
	@update_date datetime,
	@update_id uniqueidentifier
as 
	update analysis_result set 
		activity = @activity,	
		activity_uncertainty_abs = @activity_uncertainty_abs,
		activity_approved = @activity_approved,
		uniform_activity = @uniform_activity,
		uniform_activity_unit_id = @uniform_activity_unit_id,
		detection_limit = @detection_limit,
		detection_limit_approved = @detection_limit_approved,
		accredited = @accredited,
		reportable = @reportable,
		instance_status_id = @instance_status_id,
		update_date = @update_date,
		update_id = @update_id
	where id = @id
go

create proc csp_select_analysis_result
	@id uniqueidentifier
as
	select *		
	from analysis_result		
	where id = @id
go

create proc csp_select_analysis_result_flat
	@id uniqueidentifier
as	
	select 
	ar.id,
	a.number as 'analysis_number',
	n.name as 'nuclide_name',
	ar.activity,
	ar.activity_uncertainty_abs,
	ar.activity_approved,
	ar.uniform_activity,
	uau.name as 'uniform_activity_unit_name',
	ar.detection_limit,
	ar.detection_limit_approved,
	ar.accredited,
	ar.reportable,
	inst.name as 'instance_status_name',
	ar.create_date,
	ar.create_id,
	ar.update_date,
	ar.update_id
from analysis_result ar
	left outer join analysis a on a.id = ar.analysis_id
	left outer join nuclide n on n.id = ar.nuclide_id
	left outer join uniform_activity_unit uau on uau.id = ar.uniform_activity_unit_id
	left outer join instance_status inst on inst.id = ar.instance_status_id
where ar.id = @id
go

create proc csp_select_analysis_result_informative
	@id uniqueidentifier
as
	select 
	ar.id,
	a.number as 'analysis_number',
	n.name as 'nuclide_name',
	ar.activity as 'activity',
	ar.activity_uncertainty_abs as 'activity_uncertainty',
	ar.activity_approved,
	ar.uniform_activity,
	uau.name as 'uniform_activity_unit',
	ar.detection_limit,
	ar.detection_limit_approved,
	ar.accredited,
	ar.reportable,
	inst.name as 'instance_status',
	ar.create_date,
	ar.create_id,
	ar.update_date,
	ar.update_id
from analysis_result ar
	left outer join analysis a on a.id = ar.analysis_id
	left outer join nuclide n on n.id = ar.nuclide_id
	left outer join uniform_activity_unit uau on uau.id = ar.uniform_activity_unit_id
	left outer join instance_status inst on inst.id = ar.instance_status_id
where ar.id = @id
go

create proc csp_select_analysis_results_for_analysis
	@analysis_id uniqueidentifier
as
	select ar.*, n.name as 'nuclide_name'
	from analysis_result ar 
		inner join nuclide n on n.id = ar.nuclide_id
	where ar.analysis_id = @analysis_id
go

create proc csp_select_analysis_results_for_analysis_informative
	@analysis_id uniqueidentifier
as
	select
		ar.id,
		n.name as 'nuclide_name',
		ar.activity,	
		ar.activity_uncertainty_abs,
		ar.activity_approved,
		ar.uniform_activity,
		ua.name as 'uniform_activity_name',
		ar.detection_limit,
		ar.detection_limit_approved,
		ar.accredited,
		ar.reportable
	from analysis_result ar 
		inner join nuclide n on n.id = ar.nuclide_id
		left outer join	uniform_activity_unit ua on ua.id = ar.uniform_activity_unit_id
	where ar.analysis_id = @analysis_id
go

/*===========================================================================*/
/* tbl attachment */

if OBJECT_ID('dbo.attachment', 'U') is not null drop table attachment;

create table attachment (
	id uniqueidentifier primary key not null,
	source_table nvarchar(80) not null,
	source_id uniqueidentifier not null,
	name nvarchar(256) not null,
	file_extension nvarchar(16) not null,
	content varbinary(max) not null,
	create_date datetime not null,
	create_id uniqueidentifier not null	
)
go

/*===========================================================================*/
/* foreign key constraints */

alter table analysis_result add foreign key (analysis_id) references analysis(id); 
alter table analysis_result add foreign key (nuclide_id) references nuclide(id); 
alter table analysis add foreign key (preparation_id) references preparation(id); 
alter table analysis add foreign key (analysis_method_id) references analysis_method(id); 
alter table preparation add foreign key (sample_id) references sample(id); 
alter table preparation add foreign key (preparation_method_id) references preparation_method(id); 
alter table sample add foreign key (sample_type_id) references sample_type(id); 
alter table sample add foreign key (laboratory_id) references laboratory(id); 
alter table sample add foreign key (project_sub_id) references project_sub(id); 

alter table assignment_analysis_method add foreign key (assignment_preparation_method_id) references assignment_preparation_method(id); 
alter table assignment_preparation_method add foreign key (assignment_sample_type_id) references assignment_sample_type(id); 
alter table assignment_sample_type add foreign key (assignment_id) references assignment(id); 

alter table assignment add foreign key (laboratory_id) references laboratory(id); 
alter table assignment add foreign key (account_id) references account(id); 

alter table sample_x_assignment_sample_type add foreign key (sample_id) references sample(id); 
alter table sample_x_assignment_sample_type add foreign key (assignment_sample_type_id) references assignment_sample_type(id); 

alter table assignment_x_account add foreign key (assignment_id) references assignment(id);
alter table assignment_x_account add foreign key (account_id) references account(id);

alter table municipality add foreign key (county_id) references county(id);

alter table sampler add foreign key (person_id) references person(id);
alter table customer add foreign key (person_id) references person(id);
alter table account add foreign key (person_id) references person(id);
alter table account_x_role add foreign key (account_id) references account(id);
alter table account_x_role add foreign key (role_id) references role(id);

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

alter table laboratory_x_preparation_method add foreign key (laboratory_id) references laboratory(id);
alter table laboratory_x_preparation_method add foreign key (preparation_method_id) references preparation_method(id);
alter table laboratory_x_analysis_method add foreign key (laboratory_id) references laboratory(id);
alter table laboratory_x_analysis_method add foreign key (analysis_method_id) references analysis_method(id);

alter table project_sub add foreign key (project_main_id) references project_main(id);

alter table project_sub_x_account add foreign key (project_sub_id) references project_sub(id);
alter table project_sub_x_account add foreign key (account_id) references account(id);

alter table sample_component add foreign key (sample_type_id) references sample_type(id);

alter table sample_parameter add foreign key (sample_id) references sample(id);
alter table sample_parameter add foreign key (sample_parameter_name_id) references sample_parameter_name(id);

alter table sample_type_x_preparation_method add foreign key (sample_type_id) references sample_type(id);
alter table sample_type_x_preparation_method add foreign key (preparation_method_id) references preparation_method(id);

alter table preparation_method_x_analysis_method add foreign key (preparation_method_id) references preparation_method(id);
alter table preparation_method_x_analysis_method add foreign key (analysis_method_id) references analysis_method(id);

alter table analysis_method_x_nuclide add foreign key (analysis_method_id) references analysis_method(id);
alter table analysis_method_x_nuclide add foreign key (nuclide_id) references nuclide(id);

go

/*===========================================================================*/
/* unique key constraints */

alter table assignment add constraint U_Name unique(name)
alter table sample add constraint U_Number unique(number)

go