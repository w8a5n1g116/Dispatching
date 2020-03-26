namespace DAL.Model
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Runtime.Remoting.Messaging;

    public class Model : DbContext
    {
        //您的上下文已配置为从您的应用程序的配置文件(App.config 或 Web.config)
        //使用“Model”连接字符串。默认情况下，此连接字符串针对您的 LocalDb 实例上的
        //“DAL.Model.Model”数据库。
        // 
        //如果您想要针对其他数据库和/或数据库提供程序，请在应用程序配置文件中修改“Model”
        //连接字符串。
        public Model()
            : base("name=DebugModel")
        {
        }

        public Model(string connectionString)
            : base(connectionString)
        {
        }

        //为您要在模型中包含的每种实体类型都添加 DbSet。有关配置和使用 Code First  模型
        //的详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=390109。

        // public virtual DbSet<MyEntity> MyEntities { get; set; }

        public virtual DbSet<DUser> DUser { get; set; }
        public virtual DbSet<Terminal> Terminal { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<Goods> Goods { get; set; }
        public virtual DbSet<OrderGoods> OrderGoods { get; set; }
        public virtual DbSet<TerminalWXUser> TerminalWXUser { get; set; }

        public virtual DbSet<SaltOrderGoods> SaltOrderGoods { get; set; }
        public virtual DbSet<SaltOrder> SaltOrder { get; set; }
        public virtual DbSet<SaltTerminal> SaltTerminal { get; set; }
        public virtual DbSet<SaltGoods> SaltGoods { get; set; }

        public static Model GetDbContext()
        {
            // 首先先线程上下文中查看是否有已存在的DBContext  
            // 如果有那么直接返回这个，如果没有就新建   
            Model DB = CallContext.GetData("DBContext") as Model;
            if (DB == null)
            {
                DB = new Model();
                CallContext.SetData("DBContext", DB);
            }
            return DB;
        }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}

    
}

