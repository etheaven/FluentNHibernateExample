using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Configuration = NHibernate.Cfg.Configuration;

namespace hibernateTest
{
    class Program
    {
        public class Store
        {
            public virtual int Id { get; protected set; }
            public virtual string Name { get; set; }
            public virtual IList<Product> Products { get; set; }
            public virtual IList<Employee> Staff { get; set; }

            public Store()
            {
                Products = new List<Product>();
                Staff = new List<Employee>();
            }

            public virtual void AddProduct(Product product)
            {
                product.StoresStockedIn.Add(this);
                Products.Add(product);
            }

            public virtual void AddEmployee(Employee employee)
            {
                employee.Store = this;
                Staff.Add(employee);
            }
        }

        public class Product
        {
            public virtual int Id { get; protected set; }
            public virtual string Name { get; set; }
            public virtual double Price { get; set; }
            public virtual IList<Store> StoresStockedIn { get; protected set; }

            public Product()
            {
                StoresStockedIn = new List<Store>();
            }
        }
        public class Employee
        {
            public virtual int Id { get; protected set; }
            public virtual string FirstName { get; set; }
            public virtual string LastName { get; set; }
            public virtual Store Store { get; set; }
        }


        public class StoreMap : ClassMap<Store>
        {
            public StoreMap()
            {
                Id(x => x.Id);
                Map(x => x.Name);
                HasMany(x => x.Staff)
                    .Inverse()
                    .Cascade.All();
                HasManyToMany(x => x.Products)
                    .Cascade.All()
                    .Table("StoreProduct");
            }
        }
        public class ProductMap : ClassMap<Product>
        {
            public ProductMap()
            {
                Id(x => x.Id);
                Map(x => x.Name);
                Map(x => x.Price);
                HasManyToMany(x => x.StoresStockedIn)
                    .Cascade.All()
                    .Inverse()
                    .Table("StoreProduct");
            }
        }

        public class EmployeeMap : ClassMap<Employee>
        {
            public EmployeeMap()
            {
                Id(x => x.Id);
                Map(x => x.FirstName);
                Map(x => x.LastName);
                References(x => x.Store);
            }
        }

        public class SessionFactoryBuilder
        {

            //var listOfEntityMap = typeof(M).Assembly.GetTypes().Where
            //(t => t.GetInterfaces().Contains(typeof(M))).ToList();
            //var sessionFactory = SessionFactoryBuilder.BuildSessionFactory
            //(dbmsTypeAsString, connectionStringName, listOfEntityMap, withLog, create, update); 

            public static ISessionFactory BuildSessionFactory
                (string connectionStringName, bool create = false, bool update = false)
            {
                /*var fluentConfiguration = Fluently.Configure()
                    .Database(MySQLConfiguration.Standard // change sql here
                        .ConnectionString(ConfigurationManager.ConnectionStrings[connectionStringName]
                            .ConnectionString));
                var exposeConfiguration = fluentConfiguration
//                    .Mappings(m => entityMappingTypes.ForEach(e => { m.FluentMappings.Add(e); }))
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<NHibernate.Cfg.Mappings>())
                    .CurrentSessionContext("call")
                    .ExposeConfiguration(cfg => BuildSchema(cfg, create, update));
                var buildSessionFactory = exposeConfiguration
                    .BuildSessionFactory();
                return buildSessionFactory;*/

                
                

                ISessionFactory sessionFactory = Fluently.Configure()

                    .Database(MySQLConfiguration.Standard // change sql here
                        .ConnectionString(ConfigurationManager.ConnectionStrings[connectionStringName]
                            .ConnectionString).ShowSql())
                    .Mappings(m =>
                        m.FluentMappings.AddFromAssemblyOf<Program>())
                    .ExposeConfiguration(cfg => new SchemaExport(cfg)

                        .Create(false, true))
                    .BuildSessionFactory();

                /*
                ISessionFactory sessionFactory = Fluently.Configure()

                    .Database(MySQLConfiguration.Standard // change sql here
                        .ConnectionString(ConfigurationManager.ConnectionStrings[connectionStringName]
                            .ConnectionString).ShowSql())
                    .Mappings(m => m.FluentMappings
                        .AddFromAssemblyOf<TaskMap>()
                        .AddFromAssemblyOf<PersonMap>())
                    .ExposeConfiguration(cfg => new SchemaExport(cfg)

                        .Create(false, false))

                    .BuildSessionFactory();*/
                return sessionFactory;
            }

            /// <summary>
            /// Build the schema of the database.
            /// </summary>
            /// <param name="config">Configuration.</param>
            private static void BuildSchema(Configuration config, bool create = false, bool update = false)
            {
                if (create)
                {
                    new SchemaExport(config).Create(false, true);
                }
                else
                {
                    new SchemaUpdate(config).Execute(false, update);
                }
            }
        }

        public static void AddProductsToStore(Store store, params Product[] products)
        {
            foreach (var product in products)
            {
                store.AddProduct(product);
            }
        }

        public static void AddEmployeesToStore(Store store, params Employee[] employees)
        {
            foreach (var employee in employees)
            {
                store.AddEmployee(employee);
            }
        }


        private static void WriteStorePretty(Store store)
        {
            Console.WriteLine(store.Name);
            Console.WriteLine("  Products:");

            foreach (var product in store.Products)
            {
                Console.WriteLine("    " + product.Name);
            }

            Console.WriteLine("  Staff:");

            foreach (var employee in store.Staff)
            {
                Console.WriteLine("    " + employee.FirstName + " " + employee.LastName);
            }

            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            // create our NHibernate session factory
            string connectionStringName = "LocalDevelopment";
            var sessionFactory =
                SessionFactoryBuilder.BuildSessionFactory(connectionStringName, true, true);
            //var sessionFactory = Build();

            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    // create a couple of Stores each with some Products and Employees
                    var barginBasin = new Store { Name = "Bargin Basin" };
                    var superMart = new Store { Name = "SuperMart" };

                    var potatoes = new Product { Name = "Potatoes", Price = 3.60 };
                    var fish = new Product { Name = "Fish", Price = 4.49 };
                    var milk = new Product { Name = "Milk", Price = 0.79 };
                    var bread = new Product { Name = "Bread", Price = 1.29 };
                    var cheese = new Product { Name = "Cheese", Price = 2.10 };
                    var waffles = new Product { Name = "Waffles", Price = 2.41 };

                    var daisy = new Employee { FirstName = "Daisy", LastName = "Harrison" };
                    var jack = new Employee { FirstName = "Jack", LastName = "Torrance" };
                    var sue = new Employee { FirstName = "Sue", LastName = "Walkters" };
                    var bill = new Employee { FirstName = "Bill", LastName = "Taft" };
                    var joan = new Employee { FirstName = "Joan", LastName = "Pope" };

                    // add products to the stores, there's some crossover in the products in each
                    // store, because the store-product relationship is many-to-many
                    AddProductsToStore(barginBasin, potatoes, fish, milk, bread, cheese);
                    AddProductsToStore(superMart, bread, cheese, waffles);

                    // add employees to the stores, this relationship is a one-to-many, so one
                    // employee can only work at one store at a time
                    AddEmployeesToStore(barginBasin, daisy, jack, sue);
                    AddEmployeesToStore(superMart, bill, joan);

                    // save both stores, this saves everything else via cascading
                    session.SaveOrUpdate(barginBasin);
                    session.SaveOrUpdate(superMart);

                    transaction.Commit();
                }

                // retreive all stores and display them
                using (session.BeginTransaction())
                {
                    var stores = session.CreateCriteria(typeof(Store))
                      .List<Store>();

                    foreach (var store in stores)
                    {
                        WriteStorePretty(store);
                    }
                }

                Console.ReadKey();
            }
        }
    }
}
