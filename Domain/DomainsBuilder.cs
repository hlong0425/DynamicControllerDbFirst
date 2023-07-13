using Microsoft.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;

namespace Domain
{
    public static class DomainsBuilder
    {
        public static List<object> Domains { get; set; } = new List<object>();

        public static Assembly Assembly { get; set; }

        public static void GenerateObjectFromTable(string connectionString)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sql = "SELECT name from sys.tables";

                List<string> tablesName = new List<string>();
                using (var command = new SqlCommand(sql, connection))
                {
                    using SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        tablesName.Add(reader.GetString(0));
                    }
                }

                var tables = tablesName.Where(p => !p.Contains("__EFMigrationsHistory"));

                // Create a dynamic assembly
                // Create a dynamic assembly
                var assemblyName = new AssemblyName("DynamicAssembly");
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);

                // Create a dynamic module
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");

                foreach (var tableName in tables)
                {
                    List<Dictionary<string, string>> columns = GetColumnsNames(connection, tableName); // {Name: ID}, {Type: nvarchar }

                    TypeBuilder typeBuilder = moduleBuilder.DefineType(tableName, TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout);

                    // Create properties based on the table columns
                    foreach (var col in columns)
                    {
                        string propertyName = col["Name"];
                        Type propertyType = BuidPropertyTypeFromSqlType(col["Type"]);


                        FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

                        PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
                        MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
                        ILGenerator getIl = getPropMthdBldr.GetILGenerator();

                        getIl.Emit(OpCodes.Ldarg_0);
                        getIl.Emit(OpCodes.Ldfld, fieldBuilder);
                        getIl.Emit(OpCodes.Ret);

                        MethodBuilder setPropMthdBldr =
                            typeBuilder.DefineMethod("set_" + propertyName,
                              MethodAttributes.Public |
                              MethodAttributes.SpecialName |
                              MethodAttributes.HideBySig,
                              null, new[] { propertyType });

                        ILGenerator setIl = setPropMthdBldr.GetILGenerator();
                        Label modifyProperty = setIl.DefineLabel();
                        Label exitSet = setIl.DefineLabel();

                        setIl.MarkLabel(modifyProperty);
                        setIl.Emit(OpCodes.Ldarg_0);
                        setIl.Emit(OpCodes.Ldarg_1);
                        setIl.Emit(OpCodes.Stfld, fieldBuilder);

                        setIl.Emit(OpCodes.Nop);
                        setIl.MarkLabel(exitSet);
                        setIl.Emit(OpCodes.Ret);

                        propertyBuilder.SetGetMethod(getPropMthdBldr);
                        propertyBuilder.SetSetMethod(setPropMthdBldr);
                    }

                    var type = typeBuilder.CreateType();
                    Domains.Add(Activator.CreateInstance(type));
                    Assembly = type.Assembly;
                }
            }
        }

        private static List<Dictionary<string, string>> GetColumnsNames(SqlConnection connection, string tableName)
        {
            String sql = "select COLUMN_NAME, DATA_TYPE " +
                         "from INFORMATION_SCHEMA.COLUMNS " +
                         $"where TABLE_NAME = '{tableName}'";

            List<Dictionary<string, string>> Columns = new List<Dictionary<string, string>>();

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var fileName = reader.GetString(0);
                        var dataType = reader.GetString(1);

                        Dictionary<string, string> Column = new Dictionary<string, string>();
                        Column["Name"] = fileName;
                        Column["Type"] = dataType;

                        Columns.Add(Column);
                    }
                }
            }

            return Columns;
        }

        private static Type BuidPropertyTypeFromSqlType(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "bigint":
                    return typeof(long);
                case "binary":
                case "image":
                case "timestamp":
                case "varbinary":
                    return typeof(byte[]);
                case "bit":
                    return typeof(bool);
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return typeof(string);
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return typeof(DateTime);
                case "decimal":
                case "money":
                case "numeric":
                case "smallmoney":
                    return typeof(decimal);
                case "float":
                    return typeof(double);
                case "int":
                    return typeof(int);
                case "real":
                    return typeof(float);
                case "smallint":
                    return typeof(short);
                case "tinyint":
                    return typeof(byte);
                case "uniqueidentifier":
                    return typeof(Guid);
                default:
                    return null;
            }
        }
    }
}