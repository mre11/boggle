﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Net;
using static System.Net.HttpStatusCode;
using System.Configuration;

namespace BoggleService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "BoggleService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select BoggleService.svc or BoggleService.svc.cs at the Solution Explorer and start debugging.
    public class BoggleService : IBoggleService
    {
        // Boggle Service Connection String
        private static string BoggleServiceCS;

        static BoggleService()
        {
            BoggleServiceCS = ConfigurationManager.ConnectionStrings["BoggleDB"].ConnectionString;
        }

        /// <summary>
        /// Sets the status code for the next HTTP response.
        /// </summary>
        private static void SetStatus(HttpStatusCode code)
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = code;
        }

        public Stream API()
        {
            SetStatus(OK);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "index.html");
        }
        // Put
        public void CancelJoin(User user)
        {
            throw new NotImplementedException();
        }

        // Post
        public UserResponse CreateUser(User requestedUser)
        {
            InitializePendingGame();
            // Error check users information before setting up SQL connection.
            if (requestedUser.Nickname == null || requestedUser.Nickname.Trim() == "")
            {
                SetStatus(Forbidden);
                return null;
            }

            // Open SQL connection to BoggleDB
            using (SqlConnection conn = new SqlConnection(BoggleServiceCS))
            {
                conn.Open();

                // Open SQL transaction and begin transaction
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    // Open SQL command with SQL code
                    using (SqlCommand command = new SqlCommand("insert into Users(UserToken, Nickname) values(@UserToken, @Nickname)", conn, trans))
                    {
                        string userToken = Guid.NewGuid().ToString();
                        // Set command parameters with AddWithValue
                        command.Parameters.AddWithValue("@UserToken", userToken);
                        command.Parameters.AddWithValue("@Nickname", requestedUser.Nickname.Trim());

                        // Execute a non query and if successful set status code
                        // We may not need to execute non query because we already know the nickname is 
                        // correct and that the userToken will be correct.
                        command.ExecuteNonQuery();
                        SetStatus(Created);

                        // Create formatted response to send back
                        var response = new UserResponse();
                        response.UserToken = userToken;

                        // Commit users information into the database
                        trans.Commit();
                        return response;
                    }
                }

            }
        }
        // Get
        public BoggleGameResponse GameStatus(string gameID, string brief)
        {

            // Error check gameID and Brief information before setting up SQL connection.

            // Open SQL connection to BoggleDB

            // Open SQL transaction and begin transaction

            // Open SQL command with SQL code

            // Set command parameters with AddWithValue

            // Use SqlDataReader reader = command.ExecuteReader to get information of of database.

            // Execute a non query and if successful set status code

            // Commit the transaction and return the UserResponse.

            throw new NotImplementedException();
        }

        // Post
        public BoggleGameResponse JoinGame(JoinGameRequest requestBody)
        {
            // Initialize pending game

            // Make sure game is >= 5 and <= 120 and the user token is valid, otherwise setStatus to forbidden.
            if (requestBody.UserToken == null || requestBody.UserToken == ""
                || requestBody.TimeLimit < 5 || requestBody.TimeLimit > 120)
            {
                SetStatus(Forbidden);
                return null;
            }

            using (SqlConnection conn = new SqlConnection(BoggleServiceCS))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {

                }
            }

            throw new NotImplementedException();
        }
        // Put
        public BoggleWordResponse PlayWord(string gameID, BoggleWord word)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If the games record is empty, add a game as the first pending game.
        /// </summary>
        private void InitializePendingGame()
        {
            using(SqlConnection conn = new SqlConnection())
            {
                conn.Open();
                bool createPendingGame = false;

                using(SqlTransaction trans = conn.BeginTransaction())
                {
                    using(SqlCommand cmd = new SqlCommand("SELECT GameState FROM Game WHERE GameState = 'pending'", conn, trans))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if(!reader.HasRows)
                            {
                                createPendingGame = true;
                            }
                        }
                    }

                    if (createPendingGame)
                    {
                        // I changed the database so player1 can be null and only game state is populated with
                        // default value 'pending'. However we may want to change player1 to not be null. Still
                        // unsure at the moment.
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO Game DEFAULT VALUES", conn, trans))
                        {
                            trans.Commit();
                        }
                    }

                }
            }
        }
    }
}
