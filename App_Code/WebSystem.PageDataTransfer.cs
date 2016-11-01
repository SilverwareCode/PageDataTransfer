using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

/// <summary>
/// Trida slouzi k definici trid pro prenos dat mezi strankami
/// aby jsme nemuseli pouzivat POSTBACK
/// </summary>
/// 

namespace WebSystem
{
    public class Page
    {
        public Page()
        {
            //
            // TODO: Add constructor logic here
            //                       
        }

        public static string getData(string uid, bool DeleteUidAfterReading = true, string tempTableName = "_PageDataExchange", string ConnectionStringName = "DefaultConnection")
        {
            //funkce cte hodnoty z databaze podle vlozeneho UID v parametru

            //zjistujeme jestli mame UID
            if (String.IsNullOrEmpty(uid))
            {
                //neni UID
                return "getData error - no UID specified";
            }


            //pokud neni predan vlastni ConnectionString, tak zkousime pouzit DefaultConnection string z Web.Configu
            string cs = System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;

            //zjistujeme, jestli connectionString neni prazdny retezec 
            if (String.IsNullOrEmpty(cs))
            {
                //mame prazdny connection string
                //neni odkud cist data
                return "getData error - nemame ConnectionString";
            }

            //cteme data z database
            string sql = "SELECT * FROM @tempTableName WHERE uid='@uid'";

            sql = Regex.Replace(sql, "@uid", uid);
            sql = Regex.Replace(sql, "@tempTableName", tempTableName);

            //v tabulce by mel byt jen jeden zaznam
            DataTable pageData = WebSystem.Database.GetDataTable(sql, ConnectionStringName);

            string pageDataString = String.Empty;

            for (int i = 0; i < pageData.Rows.Count; i++)
            {
                pageDataString = pageData.Rows[i][1].ToString();
            }

            string deletedId = "";

            if (DeleteUidAfterReading)
            {
                //vymazavame zaznam z databaze
                sql = @"
                      DELETE FROM @tempTableName OUTPUT DELETED.Uid WHERE(Uid = '@Uid')

                      IF OBJECT_ID(N'@tempTableName', N'U') IS NOT NULL
                      BEGIN
                       --zjistujeme jestli je tabulka prazdna
                       IF (SELECT Count(*) FROM @tempTableName) = 0
	                    BEGIN
		                    --PRINT ('tabulka prazdna - mazeme tabulku')
		                    DROP TABLE @tempTableName
	                    END
                      END
                    ";

                sql = Regex.Replace(sql, "@Uid", uid);
                sql = Regex.Replace(sql, "@tempTableName", tempTableName);

                Debug.WriteLine(sql);

                deletedId = WebSystem.Database.ExecuteScalarString(sql, ConnectionStringName);

                if (deletedId == uid)
                {
                    //zaznam smazan z databaze
                }
                else
                {
                    //nepodarilo se smazat zaznam z databaze
                    return "getData chyba - nepodarilo ze vymazat UID zaznam z databaze";
                }
            }

            return pageDataString;
        }


        public static bool Open(string targetPage, string pageData, string uid = "", string ConnectionStringName = "DefaultConnection", string tempTableName = "_PageDataExchange")
        {
            //funkce otevira dalsi stranku
            //ale predtim uklada pozadovana pageData data do databaze
            //pod unikatnim ID pro pouziti na dalsi strance

            string outputedUid = "";

            if (String.IsNullOrEmpty(targetPage))
            {
                //zjistujeme jestli zname cilovou stranku
                throw new Exception("Websystem.Page.Open Cilova stranka musi byt urcena");
            }

            if (String.IsNullOrEmpty(pageData))
            {
                //pro cilovou stranku neukladame zadna data
                throw new Exception("Websystem.Page.Open Cilova stranka musi mit i ulozena data");
            }

            //pokud neni predan vlastni ConnectionString,
            //tak zkousime pouzit DefaultConnection string z Web.Configu

            string cs = System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;

            //zjistujeme, jestli connectionString neni prazdny retezec 
            if (String.IsNullOrEmpty(cs))
            {
                //mame prazdny connection string
                return false;
            }

            //pokud je uid nevyplnene, 
            //tak vytvarime svoje vlastni UID
            string sql = "";


            if (String.IsNullOrEmpty(uid))
            {
                sql = @"
                        IF OBJECT_ID(N'@PageDataExchange', N'U') IS  NULL
	                        BEGIN

		                        CREATE TABLE @PageDataExchange(
	                                                    [Uid] [uniqueidentifier] NOT NULL DEFAULT (newid()),
	                                                    [Data] [nvarchar](max) NULL,
                                                        [WhenExpire][datetime] DEFAULT DATEADD(SECOND,3600,GETDATE()),
                                                        [WhenCreated] [datetime] DEFAULT (getdate()))
	                        END
	                        DELETE FROM @PageDataExchange WHERE (WhenExpire IS NULL) OR ( GETDATE() > WhenExpire  );

	                        INSERT INTO @PageDataExchange(data)
                                                OUTPUT INSERTED.Uid
                                                VALUES ('@data')
                            ";



                sql = Regex.Replace(sql, "@PageDataExchange", tempTableName);
                sql = Regex.Replace(sql, "@data", pageData);



            }
            else
            {
                //aktualizujeme stávající záznam (UPDATE)
                Debug.WriteLine("Aktualizujeme zaznam uid = " + uid);
                sql = @"DELETE FROM @PageDataExchange WHERE (WhenExpire IS NULL) OR ( GETDATE() > WhenExpire  );
                        UPDATE @PageDataExchange  SET Data = N'@data' OUTPUT inserted.Uid WHERE Uid='@Uid'";

                sql = Regex.Replace(sql, "@PageDataExchange", tempTableName);
                sql = Regex.Replace(sql, "@data", pageData);
                sql = Regex.Replace(sql, "@Uid", uid);

                Debug.WriteLine(sql);

            }

            //z databaze vracime uid string prave vlozeneho zaznamu
            outputedUid = WebSystem.Database.ExecuteScalarString(sql, ConnectionStringName);



            if (!String.IsNullOrEmpty(outputedUid))
            {
                //pokud je zaznam uspesne vlozeny do databaze

                //tvorime query string pro cilovou stranku
                string targetUrl = targetPage + "?uid=" + outputedUid;

                //navigujeme na novou stranku
                HttpContext.Current.Response.Redirect(targetUrl, false);

                return true;
            }
            else
            {
                //zaznam se nepodarilo vlozit do databaze

                throw new Exception("Websystem.Page.Open Zaznam se nepodarilo vlozit do databaze");
                return false;
            }
        }

    }
}