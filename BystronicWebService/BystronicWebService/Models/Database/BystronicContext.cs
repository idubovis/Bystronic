using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BystronicWebService.Models.Database
{
    public partial class BystronicContext : DbContext
    {
        public BystronicContext()
        {
        }

        public BystronicContext(DbContextOptions<BystronicContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DB_Customer> Customer { get; set; }
        public virtual DbSet<DB_Order> Order { get; set; }
        public virtual DbSet<DB_OrderItem> OrderItem { get; set; }
        public virtual DbSet<DB_OrderStatus> OrderStatus { get; set; }
        public virtual DbSet<DB_Product> Product { get; set; }
        public virtual DbSet<DB_ProductType> ProductType { get; set; }
        public virtual DbSet<DB_Region> Region { get; set; }
        public virtual DbSet<DB_RoleToAccess> RoleToAccess { get; set; }
        public virtual DbSet<DB_SalesPerson> SalesPerson { get; set; }
        public virtual DbSet<DB_SalesPersonType> SalesPersonType { get; set; }
        public virtual DbSet<DB_Template> Template { get; set; }
        public virtual DbSet<DB_UsersGrid> UsersGrid { get; set; }

        // view
        public virtual DbQuery<View_SalesPeopleList> SalesPeopleList { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=lchpid1;Initial Catalog=Bystronic;Integrated Security=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           



            modelBuilder.Entity<DB_Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.Address)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.City)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RegionId).HasColumnName("RegionID");

                entity.Property(e => e.Sapnumber)
                    .HasColumnName("SAPNumber")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.State)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Zip)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DB_Order>(entity =>
            {
                entity.Property(e => e.OrderId).HasColumnName("OrderID");
                entity.HasKey(e => e.OrderId);

                entity.Property(e => e.ApprovedBy)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedDate).HasColumnType("datetime");

                entity.Property(e => e.Comments)
                    .HasMaxLength(5000)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.DateEntered)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateOfSale).HasColumnType("datetime");

                entity.Property(e => e.DealerCommission).HasColumnType("numeric(18, 4)");

                entity.Property(e => e.DiscountAmount).HasColumnType("numeric(18, 4)");

                entity.Property(e => e.DiscountPerc).HasColumnType("numeric(18, 4)");

                entity.Property(e => e.EstimatedShipDate).HasColumnType("date");

                entity.Property(e => e.FinalPaymentDate).HasColumnType("date");

                entity.Property(e => e.PaidBy)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.PaidDate).HasColumnType("datetime");

                entity.Property(e => e.PayRsmcommission).HasColumnName("PayRSMCommission");

                entity.Property(e => e.PayrollDate).HasColumnType("date");

                entity.Property(e => e.Pmcommission)
                    .HasColumnName("PMCommission")
                    .HasColumnType("numeric(18, 4)");

                entity.Property(e => e.Ponumber)
                    .HasColumnName("PONumber")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReleasedBy)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ReleasedDate).HasColumnType("datetime");

                entity.Property(e => e.Rmcommission)
                    .HasColumnName("RMCommission")
                    .HasColumnType("numeric(18, 4)");

                entity.Property(e => e.Rsmadid)
                    .HasColumnName("RSMADID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SalePrice).HasColumnType("numeric(18, 4)");

                entity.Property(e => e.SalesOrder)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SalesPerson2Adid)
                    .HasColumnName("SalesPerson2ADID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SalesPersonAdid)
                    .HasColumnName("SalesPersonADID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SalesPersonId).HasColumnName("SalesPersonID");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TemplateId).HasColumnName("TemplateID");

                entity.Property(e => e.TotalProductCost).HasColumnType("numeric(18, 4)");

                entity.Property(e => e.TrackingNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Order)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_Order_Customer");
            });

            modelBuilder.Entity<DB_OrderItem>(entity =>
            {
                entity.HasKey(e => e.OrderItemId);

                entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");

                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.ListPrice).HasColumnType("numeric(18, 4)");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.ToolingPrice).HasColumnType("numeric(18, 4)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItem)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_OrderItem_Order");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderItem)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderItem_Product");
            });

            modelBuilder.Entity<DB_OrderStatus>(entity =>
            {
                entity.HasKey(e => e.OrderStatusId);
                entity.Property(e => e.OrderStatusId).HasColumnName("OrderStatusID");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DB_Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.Name)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("numeric(18, 4)");

                entity.Property(e => e.ProductManager)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProductTypeId).HasColumnName("ProductTypeID");

                entity.HasOne(d => d.ProductType)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.ProductTypeId)
                    .HasConstraintName("FK_Product_ProductType");
            });

            modelBuilder.Entity<DB_ProductType>(entity =>
            {
                entity.HasKey(e => e.ProductTypeId);
                entity.Property(e => e.ProductTypeId).HasColumnName("ProductTypeID");

                entity.Property(e => e.CommissionPerc).HasColumnType("numeric(18, 4)");

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DB_Region>(entity =>
            {
                entity.HasKey(e => e.RegionId);
                entity.Property(e => e.RegionId).HasColumnName("RegionID");

                entity.Property(e => e.Region1)
                    .HasColumnName("Region")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DB_RoleToAccess>(entity =>
            {
                entity.HasKey(e => e.RoleToAccessId);
                entity.Property(e => e.RoleToAccessId).HasColumnName("RoleToAccessID");

                entity.Property(e => e.Adrole)
                    .HasColumnName("ADRole")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AppAccess)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DB_SalesPerson>(entity =>
            {
                entity.HasKey(e => e.Adid);

                entity.Property(e => e.Adid)
                    .HasColumnName("ADID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.ClientAdid)
                    .HasColumnName("ClientADID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CostCenter)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.RegionId).HasColumnName("RegionID");

                entity.Property(e => e.SalesGoalAmount).HasColumnType("numeric(18, 0)");

                entity.Property(e => e.SalesGoals)
                    .HasMaxLength(5000)
                    .IsUnicode(false);

                entity.Property(e => e.SalesPersonId)
                    .HasColumnName("SalesPersonID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.SalesRepId)
                    .HasColumnName("SalesRepID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.Property(e => e.YtdsalesAmountBaseline)
                    .HasColumnName("YTDSalesAmountBaseline")
                    .HasColumnType("numeric(18, 4)");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.SalesPerson)
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("FK_SalesPerson_Region");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.SalesPerson)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK_SalesPerson_SalesPersonType");
            });

            modelBuilder.Entity<DB_SalesPersonType>(entity =>
            {
                entity.HasKey(e => e.SalesPersonTypeId);
                entity.Property(e => e.SalesPersonTypeId).HasColumnName("SalesPersonTypeID");

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DB_Template>(entity =>
            {
                entity.HasKey(e => e.TemplateId);
                entity.Property(e => e.TemplateId).HasColumnName("TemplateID");

                entity.Property(e => e.Formula).IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Pmadid)
                    .HasColumnName("PMADID")
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DB_UsersGrid>(entity =>
            {
                entity.HasKey(e => e.UserName);

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.GridName)
                   .HasMaxLength(50)
                   .IsUnicode(false)
                   .ValueGeneratedNever();

                entity.Property(e => e.GridColumns)
                    .HasMaxLength(1500)
                    .IsUnicode(false);
            });

            modelBuilder.Query<View_SalesPeopleList>().ToView("SalesPeopleList");
        }
    }
}
