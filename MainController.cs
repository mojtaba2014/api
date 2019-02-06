using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.IO.Pipes;
using System.Security.Principal;
using NolekClient.CommunicationWithService;
using System.Threading;
using NolekClient.Model;
using System.Globalization;

namespace NolekClient.Controllers
{
    public class MainController
    {
        string messageFromService = "";
        //bool statusSendStrToService = true;
        string statusMessage;
        NamedPipeClientStream pipeClient;
        public List<Model.ServerClass> connectionStringList;
        List<StoredColumn> Columns;

        private static readonly MainController instanceContr = new MainController();
        /// <summary>
        /// We used the Singleton Pattern 
        /// to create an instance of the Controller
        /// </summary>
        static MainController()
        { }
        public MainController()
        {
            Columns = new List<StoredColumn>();
            CreateNewPipe();
            connectionStringList = new List<ServerClass>();
        }

        public static MainController Instance
        {
            get
            {
                return instanceContr;
            }
        }

        /// <summary>
        /// Create a new pipe with the Windows Service
        /// </summary>
        public void CreateNewPipe()
        {
            this.pipeClient =
        new NamedPipeClientStream(".", "testpipe",
        PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

        }

        /// <summary>
        /// The method which connects to the pipe
        /// </summary>
        public void ConnectToService()
        {
            try
            {
                pipeClient.Connect();

                StringStream stringStream = new StringStream(pipeClient);
                if (stringStream.ReadStringFromService() == "I am the one true server!")
                {
                }
                else
                {
                    Console.WriteLine("Server could not be verified.");
                }

            }
            catch (Exception ex)
            {
                string gi = ex.Message;
                throw;
            }

        }

        /// <summary>
        /// Close the connection with the connected pipe
        /// </summary>
        public void DisconnectFromService()
        {
            StringStream stringStream = new StringStream(pipeClient);

            stringStream.WriteStringToService("Close");
            ConnectToService();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="dbName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string SendConStrToService(string serverName, string dbName, string userName, string password)
        {
            string configeDB = "SetNewConnectionString";
            string result = "";
            try
            {

                StringStream stringStream = new StringStream(pipeClient);

                stringStream.WriteStringToService(configeDB);
                stringStream.WriteStringToService(serverName);
                stringStream.WriteStringToService(dbName);
                stringStream.WriteStringToService(userName);
                stringStream.WriteStringToService(password);

                result = stringStream.ReadStringFromService();

            }
            catch (Exception e)
            {
                result = e.Message.ToString();
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="dbName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="oldstring"></param>
        /// <returns></returns>
        public string SendEditConStrToService(string serverName, string dbName, string userName, string password, string oldstring)
        {
            string doWhat = "EditConnectionString";
            string result = "";
            try
            {
                StringStream stringStream = new StringStream(pipeClient);

                stringStream.WriteStringToService(doWhat);
                stringStream.WriteStringToService(serverName);
                stringStream.WriteStringToService(dbName);
                stringStream.WriteStringToService(userName);
                stringStream.WriteStringToService(password);
                stringStream.WriteStringToService(oldstring);

                result = stringStream.ReadStringFromService();
            }
            catch (Exception e)
            {
                return e.Message.ToString();
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <returns></returns>
        public string DeleteConStr(string connectionstring)
        {
            string doWhat = "DeleteConnectionString";
            string result = "";
            try
            {
                StringStream stringStream = new StringStream(pipeClient);

                stringStream.WriteStringToService(doWhat);
                stringStream.WriteStringToService(connectionstring);

                result = stringStream.ReadStringFromService();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return result;

        }



        /// <summary>
        /// Here we checking if our Service is running
        /// </summary>
        /// <returns>The status of the Service</returns>
        public string CheckStatus()
        {
            try
            {
                ServiceController[] scServices;
                scServices = ServiceController.GetServices();

                ServiceController serviceController = new ServiceController("Nolek Service");
                foreach (ServiceController scTemp in scServices)
                {

                    if (scTemp.ServiceName == "Nolek Service")
                    {

                        if (serviceController.Status == ServiceControllerStatus.Stopped)
                        {
                            if (serviceController.Status == ServiceControllerStatus.Stopped)
                            {
                                statusMessage = "The Service is not running";
                                scTemp.Start();
                            }
                        }
                        else if (serviceController.Status == ServiceControllerStatus.Running)
                        {
                            if (serviceController.Status == ServiceControllerStatus.Running)
                            {
                                statusMessage = "The Service is running";
                            }
                        }
                    }
                }
            }
            catch (Exception exe)
            {
                statusMessage = exe.Message.ToString();
            }

            return statusMessage;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string AskForConnStr()
        {
            connectionStringList.Clear();

            StringStream stringStream = new StringStream(pipeClient);
            string sendRequest = "GetConnectionStrings";
            stringStream.WriteStringToService(sendRequest);
            string amountOfStringsOrError = stringStream.ReadStringFromService();
            List<string> connectionList = new List<string>();
            if (amountOfStringsOrError.Contains("Error"))
            {
                return amountOfStringsOrError;
            }
            else
            {
                int amount = int.Parse(amountOfStringsOrError);
                for (int i = 0; i < amount; i++)
                {
                    connectionList.Add(stringStream.ReadStringFromService());
                }
                foreach (string str in connectionList)
                {
                    connectionStringList.Add(new Model.ServerClass(str.Split('=', ';')[1],
                                           new Model.ConnectionString(str.Split('=', ';')[3],
                                                                      str.Split('=', ';')[5],
                                                                      str.Split('=', ';')[7])));
                }
                return "True";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        public string ConnectToDatabase(string ServerName, string DbName, string UsrName, string Password)
        {
            try
            {
                StringStream stringStream = new StringStream(pipeClient);
                string sendProtocol = "ConnectToDatabase";
                stringStream.WriteStringToService(sendProtocol);

                stringStream.WriteStringToService(ServerName);
                stringStream.WriteStringToService(DbName);
                stringStream.WriteStringToService(UsrName);
                stringStream.WriteStringToService(Password);
                messageFromService = stringStream.ReadStringFromService();
            }
            catch (Exception exceptionInConnectToDB)
            {
                messageFromService = exceptionInConnectToDB.Message;
            }
           
            return messageFromService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string DisconnectFromDatabase()
        {
            try
            {
                StringStream stringStream = new StringStream(pipeClient);
                string sendProtocol = "DisconnectFromDatabase";
                stringStream.WriteStringToService(sendProtocol);

                messageFromService = stringStream.ReadStringFromService();
            }
            catch (Exception exceptionInDisconnectFromDB)
            {
                messageFromService = exceptionInDisconnectFromDB.Message;
            }
            return messageFromService;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DBReturnValues AddToListOfReceptValuesFromService(string SPName, string materialNo)
        {
            StringStream stringStream = new StringStream(pipeClient);
            string sendProtocol = "ReceiveData";
            stringStream.WriteStringToService(sendProtocol);
            stringStream.WriteStringToService(SPName);
            stringStream.WriteStringToService(materialNo);
            string knownColumn = "false";
            StoredColumn myColumn = new StoredColumn();
            DBReturnValues newValues = new DBReturnValues();
            try
            {
                
                foreach (StoredColumn item in Columns)
                {
                    if (item.Name == SPName)
                    {
                        knownColumn = "true";
                        myColumn = item;
                    }
                }
                stringStream.WriteStringToService(knownColumn);

                string amountOfRowsOrError = stringStream.ReadStringFromService();

                if (amountOfRowsOrError.Contains("Error"))
                {
                    //allRows.Add(new List<DynamicClass>().Add(new DynamicClass(receivedFromService, null, null)));
                    
                    newValues.Result = amountOfRowsOrError;
                    return newValues;
                }
                else
                {
                    int amountOfRows = int.Parse(amountOfRowsOrError);
                    
                    if (knownColumn == "true")
                    {
                        for (int i = 0; i < amountOfRows; i++)
                        {
                            List<string> oneRow = new List<string>();
                            for (int j = 0; j < myColumn.ColumnNames.Count; j++)
                            {
                                oneRow.Add(stringStream.ReadStringFromService());
                            }
                            newValues.Rows.Add(oneRow);
                        } // only addded the values from the service
                          //adding the columns and the types from the saved list
                        newValues.Result = "True";
                        newValues.ColumnNames = myColumn.ColumnNames;
                        newValues.Types = myColumn.ColumnTypes;
                    }
                    else
                    {
                        //return everything from the service and save the column names and 
                        //the types into the list of known columns
                        int amountOfColumns = int.Parse(stringStream.ReadStringFromService());

                        for (int i = 0; i < amountOfColumns; i++)
                        {
                            string type = stringStream.ReadStringFromService();
                            string columnName = stringStream.ReadStringFromService();
                            newValues.ColumnNames.Add(columnName);
                            newValues.Types.Add(type);
                        } //loaded column names and types

                        //save this into the list of known columns
                        StoredColumn newStoredColumn = new StoredColumn();
                        newStoredColumn.Name = SPName;
                        newStoredColumn.ColumnNames = newValues.ColumnNames;
                        newStoredColumn.ColumnTypes = newValues.Types;
                        Columns.Add(newStoredColumn);
                        newValues.Result = "True";
                        for (int i = 0; i < amountOfRows; i++)
                        {
                            List<string> oneRow = new List<string>();
                            for (int j = 0; j < newValues.ColumnNames.Count; j++)
                            {
                                oneRow.Add(stringStream.ReadStringFromService());
                            }
                            newValues.Rows.Add(oneRow);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return newValues;
        }

        List<Result> listOfResult;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Result> ProduceFakeData()
        {
            listOfResult = new List<Result>();
            Random random = new Random();

            Result resultClass = new Result();



            resultClass.SG10IG001IN0001 = "Data" + random.Next(200);
            resultClass.SG10IG001IN0002 = random.Next(9999);
            resultClass.SG10IG001IN0003 = random.Next(9999);
            resultClass.SG10IG002IN0001 = random.Next(9999);
            resultClass.SG10IG002IN0002 = random.Next(9999);
            resultClass.SG10IG002IN0006 = random.Next(9999);
            resultClass.SG10IG002IN0007 = random.Next(9999);
            resultClass.SG10IG003IN0002 = random.Next(9999);
            resultClass.SG10IG003IN0003 = random.Next(9999);
            resultClass.SG10IG005IN0001 = random.Next(9999);
            resultClass.SG10IG005IN0002 = random.Next(9999);
            resultClass.SG10IG005IN0003 = random.Next(9999);
            resultClass.SG10IG010IN0001 = random.Next(9999);
            resultClass.SG10IG010IN0002 = random.Next(9999);
            resultClass.SG10IG010IN0003 = random.Next(9999);
            resultClass.SG10IG010IN0004 = random.Next(9999);
            resultClass.SG10IG010IN0005 = random.Next(9999);
            resultClass.SG10IG010IN0006 = random.Next(9999);
            resultClass.SG10IG010IN0007 = random.Next(9999);
            resultClass.SG10IG010IN0010 = random.Next(9999);
            resultClass.SG10IG010IN0012 = random.Next(9999);
            resultClass.SG10IG010IN0013 = random.Next(9999);
            resultClass.SG10IG010IN0014 = random.Next(9999);
            resultClass.SG10IG010IN0015 = random.Next(9999);
            resultClass.SG10IG010IN0016 = random.Next(9999);
            resultClass.SG10IG010IN0017 = random.Next(9999);
            resultClass.SG10IG050IN0001 = random.Next(9999);
            resultClass.SG10IG050IN0002 = random.Next(9999);
            resultClass.SG10IG050IN0009 = random.Next(9999);
            resultClass.SG10IG050IN0010 = (float)random.NextDouble();
            resultClass.SG10IG051IN0001 = random.Next(9999);
            resultClass.SG10IG051IN0002 = random.Next(9999);
            resultClass.SG10IG051IN0010 = (float)random.NextDouble();
            resultClass.SG10IG051IN0011 = (float)random.NextDouble();
            resultClass.SG10IG051IN0012 = (double)random.NextDouble();
            resultClass.SG10IG051IN0013 = (float)random.NextDouble();
            resultClass.SG10IG051IN0014 = (double)random.NextDouble();
            resultClass.SG10IG051IN0015 = (float)random.NextDouble();
            resultClass.SG10IG051IN0016 = (double)random.NextDouble();
            resultClass.SG10IG051IN0017 = (float)random.NextDouble();
            resultClass.SG10IG051IN0018 = (double)random.NextDouble();
            resultClass.SG10IG052IN0001 = random.Next(9999);
            resultClass.SG10IG052IN0002 = random.Next(9999);
            resultClass.SG10IG052IN0010 = random.Next(9999);
            resultClass.SG10IG052IN0011 = (float)random.NextDouble();
            resultClass.SG10IG053IN0001 = random.Next(9999);
            resultClass.SG10IG053IN0002 = random.Next(9999);
            resultClass.SG10IG053IN0015 = random.Next(9999);
            resultClass.SG10IG053IN0016 = random.Next(9999);
            resultClass.SG10IG054IN0001 = random.Next(9999);
            resultClass.SG10IG054IN0002 = random.Next(9999);
            resultClass.SG10IG054IN0010 = random.Next(9999);
            resultClass.SG10IG055IN0001 = random.Next(9999);
            resultClass.SG10IG055IN0002 = random.Next(9999);
            resultClass.SG10IG055IN0010 = (float)random.NextDouble();
            resultClass.SG10IG055IN0011 = random.Next(9999);
            resultClass.SG10IG055IN0016 = random.Next(9999);
            resultClass.SG10IG055IN0017 = random.Next(9999);
            resultClass.SG10IG055IN0018 = random.Next(9999);
            resultClass.SG10IG056IN0001 = random.Next(9999);
            resultClass.SG10IG056IN0002 = random.Next(9999);
            resultClass.SG10IG056IN0010 = (float)random.NextDouble();
            resultClass.SG10IG056IN0011 = random.Next(9999);
            resultClass.SG10IG056IN0013 = random.Next(9999);
            resultClass.SG10IG056IN0017 = random.Next(9999);
            resultClass.SG10IG056IN0018 = random.Next(9999);
            resultClass.SG10IG057IN0001 = random.Next(9999);
            resultClass.SG10IG057IN0002 = random.Next(9999);
            resultClass.SG10IG057IN0010 = random.Next(9999);
            resultClass.SG10IG057IN0011 = random.Next(9999);
            resultClass.SG10IG057IN0014 = random.Next(9999);
            resultClass.SG10IG057IN0020 = random.Next(9999);
            resultClass.SG10IG057IN0021 = random.Next(9999);
            resultClass.SG10IG058IN0001 = random.Next(9999);
            resultClass.SG10IG058IN0002 = random.Next(9999);
            resultClass.SG10IG058IN0010 = (float)random.NextDouble();
            resultClass.SG10IG058IN0011 = random.Next(9999);
            resultClass.SG10IG058IN0014 = (float)random.NextDouble();
            resultClass.SG10IG058IN0020 = random.Next(9999);
            resultClass.SG10IG058IN0021 = random.Next(9999);
            resultClass.SG10IG059IN0001 = random.Next(9999);
            resultClass.SG10IG059IN0002 = random.Next(9999);
            resultClass.SG10IG059IN0010 = random.Next(9999);
            resultClass.SG10IG059IN0011 = random.Next(9999);
            resultClass.SG10IG060IN0001 = random.Next(9999);
            resultClass.SG10IG060IN0002 = random.Next(9999);
            resultClass.SG10IG060IN0010 = (float)random.NextDouble();
            resultClass.SG10IG061IN0001 = random.Next(9999);
            resultClass.SG10IG061IN0002 = random.Next(9999);
            resultClass.SG10IG061IN0010 = random.Next(9999);
            resultClass.SG10IG061IN0011 = random.Next(9999);
            resultClass.SG10IG061IN0016 = random.Next(9999);
            resultClass.SG10IG061IN0017 = random.Next(9999);
            resultClass.SG10IG061IN0018 = random.Next(9999);
            resultClass.SG10IG062IN0001 = random.Next(9999);
            resultClass.SG10IG062IN0002 = random.Next(9999);
            resultClass.SG10IG062IN0010 = random.Next(9999);
            resultClass.SG10IG062IN0011 = (float)random.NextDouble();
            resultClass.SG10IG062IN0013 = random.Next(9999);
            resultClass.SG10IG062IN0017 = random.Next(9999);
            resultClass.SG10IG062IN0018 = random.Next(9999);
            resultClass.SG10IG063IN0001 = random.Next(9999);
            resultClass.SG10IG063IN0002 = random.Next(9999);
            resultClass.SG10IG063IN0010 = (float)random.NextDouble();
            resultClass.SG10IG063IN0011 = random.Next(9999);
            resultClass.SG10IG063IN0014 = (float)random.NextDouble();
            resultClass.SG10IG063IN0020 = random.Next(9999);
            resultClass.SG10IG063IN0021 = random.Next(9999);
            resultClass.SG10IG064IN0001 = random.Next(9999);
            resultClass.SG10IG064IN0002 = random.Next(9999);
            resultClass.SG10IG064IN0010 = (float)random.NextDouble();
            resultClass.SG10IG064IN0011 = random.Next(9999);
            resultClass.SG10IG064IN0014 = (float)random.NextDouble();
            resultClass.SG10IG064IN0020 = random.Next(9999);
            resultClass.SG10IG064IN0021 = random.Next(9999);
            resultClass.SG10IG065IN0001 = random.Next(9999);
            resultClass.SG10IG065IN0002 = random.Next(9999);
            resultClass.SG10IG066IN0001 = random.Next(9999);
            resultClass.SG10IG066IN0002 = random.Next(9999);

            listOfResult.Add(resultClass);


            return listOfResult;
        }

        string statusMessageForInsert = "";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string SendDataToService()
        {
            try
            {
                StringStream stringStream = new StringStream(pipeClient);

                //Change the protocol to insert data to SQL
                string sendProtocol = "UploadData";
                stringStream.WriteStringToService(sendProtocol);
                foreach (var item in ProduceFakeData())
                {
                    stringStream.WriteStringToService(item.SG10IG001IN0001);
                    stringStream.WriteStringToService(item.SG10IG001IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG001IN0003.ToString());
                    stringStream.WriteStringToService(item.SG10IG002IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG002IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG002IN0006.ToString());
                    stringStream.WriteStringToService(item.SG10IG002IN0007.ToString());
                    stringStream.WriteStringToService(item.SG10IG003IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG003IN0003.ToString());
                    stringStream.WriteStringToService(item.SG10IG005IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG005IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG005IN0003.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0003.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0004.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0005.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0006.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0007.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0012.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0013.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0014.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0015.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0016.ToString());
                    stringStream.WriteStringToService(item.SG10IG010IN0017.ToString());
                    stringStream.WriteStringToService(item.SG10IG050IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG050IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG050IN0009.ToString());
                    stringStream.WriteStringToService(item.SG10IG050IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG051IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG051IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG051IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG051IN0011.ToString());
                    stringStream.WriteStringToService(item.SG10IG051IN0012.ToString());
                    stringStream.WriteStringToService(item.SG10IG051IN0013.ToString());
                    stringStream.WriteStringToService(item.SG10IG051IN0014.ToString());
                    stringStream.WriteStringToService(item.SG10IG051IN0015.ToString());
                    stringStream.WriteStringToService(item.SG10IG051IN0016.ToString());
                    stringStream.WriteStringToService(item.SG10IG051IN0017.ToString());
                    stringStream.WriteStringToService(item.SG10IG051IN0018.ToString());
                    stringStream.WriteStringToService(item.SG10IG052IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG052IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG052IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG052IN0011.ToString());
                    stringStream.WriteStringToService(item.SG10IG053IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG053IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG053IN0015.ToString());
                    stringStream.WriteStringToService(item.SG10IG053IN0016.ToString());
                    stringStream.WriteStringToService(item.SG10IG054IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG054IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG054IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG055IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG055IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG055IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG055IN0011.ToString());
                    stringStream.WriteStringToService(item.SG10IG055IN0016.ToString());
                    stringStream.WriteStringToService(item.SG10IG055IN0017.ToString());
                    stringStream.WriteStringToService(item.SG10IG055IN0018.ToString());
                    stringStream.WriteStringToService(item.SG10IG056IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG056IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG056IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG056IN0011.ToString());
                    stringStream.WriteStringToService(item.SG10IG056IN0013.ToString());
                    stringStream.WriteStringToService(item.SG10IG056IN0017.ToString());
                    stringStream.WriteStringToService(item.SG10IG056IN0018.ToString());
                    stringStream.WriteStringToService(item.SG10IG057IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG057IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG057IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG057IN0011.ToString());
                    stringStream.WriteStringToService(item.SG10IG057IN0014.ToString());
                    stringStream.WriteStringToService(item.SG10IG057IN0020.ToString());
                    stringStream.WriteStringToService(item.SG10IG057IN0021.ToString());
                    stringStream.WriteStringToService(item.SG10IG058IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG058IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG058IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG058IN0011.ToString());
                    stringStream.WriteStringToService(item.SG10IG058IN0014.ToString());
                    stringStream.WriteStringToService(item.SG10IG058IN0020.ToString());
                    stringStream.WriteStringToService(item.SG10IG058IN0021.ToString());
                    stringStream.WriteStringToService(item.SG10IG059IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG059IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG059IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG059IN0011.ToString());
                    stringStream.WriteStringToService(item.SG10IG060IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG060IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG060IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG061IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG061IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG061IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG061IN0011.ToString());
                    stringStream.WriteStringToService(item.SG10IG061IN0016.ToString());
                    stringStream.WriteStringToService(item.SG10IG061IN0017.ToString());
                    stringStream.WriteStringToService(item.SG10IG061IN0018.ToString());
                    stringStream.WriteStringToService(item.SG10IG062IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG062IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG062IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG062IN0011.ToString());
                    stringStream.WriteStringToService(item.SG10IG062IN0013.ToString());
                    stringStream.WriteStringToService(item.SG10IG062IN0017.ToString());
                    stringStream.WriteStringToService(item.SG10IG062IN0018.ToString());
                    stringStream.WriteStringToService(item.SG10IG063IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG063IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG063IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG063IN0011.ToString());
                    stringStream.WriteStringToService(item.SG10IG063IN0014.ToString());
                    stringStream.WriteStringToService(item.SG10IG063IN0020.ToString());
                    stringStream.WriteStringToService(item.SG10IG063IN0021.ToString());
                    stringStream.WriteStringToService(item.SG10IG064IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG064IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG064IN0010.ToString());
                    stringStream.WriteStringToService(item.SG10IG064IN0011.ToString());
                    stringStream.WriteStringToService(item.SG10IG064IN0014.ToString());
                    stringStream.WriteStringToService(item.SG10IG064IN0020.ToString());
                    stringStream.WriteStringToService(item.SG10IG064IN0021.ToString());
                    stringStream.WriteStringToService(item.SG10IG065IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG065IN0002.ToString());
                    stringStream.WriteStringToService(item.SG10IG066IN0001.ToString());
                    stringStream.WriteStringToService(item.SG10IG066IN0002.ToString());
                }
                statusMessageForInsert = stringStream.ReadStringFromService();
            }
            catch (Exception exceptionInSendGeneratedDataToService)
            {
                statusMessageForInsert = exceptionInSendGeneratedDataToService.Message;
            }
          
            return statusMessageForInsert;
        }

        List<string> listOfConditionOfLog;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetLogging()
        {
            listOfConditionOfLog = new List<string>();

            StringStream stringStream = new StringStream(pipeClient);
            string protocol = "GetLogSettings";
            stringStream.WriteStringToService(protocol);

            string readFromService = stringStream.ReadStringFromService();
            string condition = "";
            if (readFromService.Contains("Success"))
            {
                for (int i = 0; i < 12; i++)
                {
                    condition = stringStream.ReadStringFromService();
                    listOfConditionOfLog.Add(condition);
                }
            }
            else
            {
                condition = "Error";
                listOfConditionOfLog.Add(condition);
            }

            return listOfConditionOfLog;
        }

        string LogStatus;
        public string SetLogFiles(List<string> selectedCheckBoxes)
        {
            try
            {
                StringStream stringStream = new StringStream(pipeClient);
                string protocol = "EditLogSettings";
                stringStream.WriteStringToService(protocol);

                foreach (var item in selectedCheckBoxes)
                {
                    stringStream.WriteStringToService(item);
                }

                LogStatus = stringStream.ReadStringFromService();
            }
            catch (Exception exceptionInSetLog)
            {
                LogStatus = exceptionInSetLog.Message;
            }
            return LogStatus;
        }
    }
}
