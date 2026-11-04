using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace TR.MySql.EF
{
    public class DbInlitializer : DropCreateDatabaseIfModelChanges<DbContainer>
    {
        protected override void Seed(DbContainer context)
        {         
            base.Seed(context);
        }
    }
}