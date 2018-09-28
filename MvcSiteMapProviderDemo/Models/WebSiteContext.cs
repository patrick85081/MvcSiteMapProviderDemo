using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace MvcSiteMapProviderDemo.Models
{
    public class WebSiteContext : DbContext
    {
        public WebSiteContext() : base("WebSite")
        {
            Database.SetInitializer(new DbInitialize());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Menu> Menus { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var userTable = modelBuilder.Entity<User>();
            var roleTable = modelBuilder.Entity<Role>();

            userTable.HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .Map(mc =>
                {
                    mc.ToTable("UsersRoles");
                    mc.MapLeftKey("RoleId");
                    mc.MapRightKey("MenuId");
                });

            roleTable.HasMany(r => r.Menus)
                .WithMany(m => m.Roles)
                .Map(mc =>
                {
                    mc.ToTable("RolesMenus");
                    mc.MapLeftKey("MenuId");
                    mc.MapRightKey("RoleId");
                });

            base.OnModelCreating(modelBuilder);
        }
    }

    public class DbInitialize : DropCreateDatabaseIfModelChanges<WebSiteContext>
    {
        protected override void Seed(WebSiteContext context)
        {
            base.Seed(context);

            context.Roles.AddOrUpdate(
                r=>r.RoleId,
                new Role(){Name = "Admin", IsEnable = true},
                new Role(){Name = "Staff", IsEnable = true});

            context.Users.AddOrUpdate(
                u=>u.UserId,
                new User()
                {
                    UserId = "Foo",
                    Email = "foo@ooxx.com",
                    Name = "Foo chen",
                    Password = "123",
                    IsEnable = true,
                    RegisterOn = DateTime.Now,
                    Roles = context.Roles.ToList()
                });

            context.SaveChanges();

            var menus = new List<Menu>
            {
                new Menu { MenuId = 1,   Name = "訂購", Description="訂購", Controller="",
                    Action="", RouteValues=null, ParentId=null, OrderSerial=1, Status=1, Url="#1"},
                new Menu { MenuId = 2,   Name = "維護", Description="維護", Controller="",
                    Action="", RouteValues=null, ParentId=null, OrderSerial=1, Status=1, Url="#2"},
                new Menu { MenuId = 3,   Name = "訂購模式A", Description="訂購模式A", Controller="Order",
                    Action="Create", RouteValues="orderType=0", ParentId=1, OrderSerial=1, Status=1, Url=null},
                new Menu { MenuId = 4,   Name = "訂購模式B", Description="訂購模式B", Controller="Order",
                    Action="Create", RouteValues="orderType=1", ParentId=1, OrderSerial=1, Status=1, Url=null},
                new Menu { MenuId = 5,   Name = "編輯帳號", Description="編輯帳號", Controller="User",
                    Action="Edit", RouteValues=null, ParentId=2, OrderSerial=1, Status=1, Url=null},
            };
            menus.ForEach(s => context.Menus.AddOrUpdate(p => p.MenuId, s));
            context.SaveChanges();

            AddOrUpdateMenu(context, "Admin", "訂購");
            AddOrUpdateMenu(context, "Admin", "維護");
            AddOrUpdateMenu(context, "Admin", "訂購模式A");
            AddOrUpdateMenu(context, "Admin", "訂購模式B");
            AddOrUpdateMenu(context, "Admin", "編輯帳號");

            AddOrUpdateMenu(context, "Staff", "訂購");
            AddOrUpdateMenu(context, "Staff", "訂購模式A");
            AddOrUpdateMenu(context, "Staff", "訂購模式B");
            context.SaveChanges();
        }

        void AddOrUpdateMenu(WebSiteContext context, string roleName, string menuName)
        {
            var crs = context.Roles.SingleOrDefault(c => c.Name == roleName);

            if (crs.Menus == null)
            {
                crs.Menus = new List<Menu>();
                crs.Menus.Add(context.Menus.Single(i => i.Name == menuName));
            }
            else
            {
                var inst = crs.Menus.SingleOrDefault(i => i.Name == menuName);
                if (inst == null)
                    crs.Menus.Add(context.Menus.Single(i => i.Name == menuName));
            }
        }

    }


    public class User
    {
        [Key] public string UserId { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public DateTime RegisterOn { get; set; }

        public bool IsEnable { get; set; }


        // Navigation Properties
        public virtual ICollection<Role> Roles { get; set; }
    }

    public class Role
    {
        // Properties
        [Key]
        public int RoleId { get; set; }

        public string Name { get; set; }

        public bool IsEnable { get; set; }


        // Navigation Properties
        public virtual ICollection<User> Users { get; set; }

        public virtual ICollection<Menu> Menus { get; set; }
    }

   public class Menu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MenuId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }

        public int? ParentId { get; set; }

        public int Status { get; set; }

        public string RouteValues { get; set; }

        public int? OrderSerial { get; set; }


        // Navigation Properties
        public virtual ICollection<Role> Roles { get; set; }
    }
}