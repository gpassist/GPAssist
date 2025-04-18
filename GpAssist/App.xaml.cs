using GpAssist.Services;
using GpAssist.Models;


namespace GpAssist
{
    public partial class App : Application
    {

        public static DatabaseService Database { get; private set; }

        public static  User CurrentUser { get; set; }

        public App()
        {
            InitializeComponent();

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "appointments.db");


            Database = new DatabaseService(dbPath);

            Task.Run(async () => await Database.SeedDatabaseAsync()).Wait();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}