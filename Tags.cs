using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSA_lims
{
    public class Tag<I, N>
    {        
        public Tag()
        {
        }

        public Tag(I id, N name)
        {
            Id = id;
            Name = name;
        }

        public I Id { get; set; }
        public N Name { get; set; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    public class SampleComponentTag
    {
        public SampleComponentTag()
        {
        }

        public SampleComponentTag(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public SampleComponentTag(SqlDataReader reader)
        {
            if (!reader.HasRows)
                throw new Exception("SqlDataReader has no rows in SampleComponent(SqlDataReader reader)");

            Id = new Guid(reader["id"].ToString());
            Name = reader["name"].ToString();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }

    /*public class DecayTypeTag
    {
        public DecayTypeTag()
        {
        }

        public DecayTypeTag(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }*/

    /*public class PreparationUnitTag
    {
        public PreparationUnitTag()
        {
        }

        public PreparationUnitTag(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }*/

    /*public class ActivityUnitTag
    {
        public ActivityUnitTag()
        {
        }

        public ActivityUnitTag(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }*/

   /* public class UniformActivityUnitTag
    {
        public UniformActivityUnitTag()
        {
        }

        public UniformActivityUnitTag(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }*/

    /*public class WorkflowStatusTag
    {
        public WorkflowStatusTag()
        {
        }

        public WorkflowStatusTag(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }*/

    /*public class SamplerTag
    {
        public SamplerTag()
        {
        }

        public SamplerTag(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }*/
}
