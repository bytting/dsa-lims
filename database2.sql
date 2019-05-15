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
