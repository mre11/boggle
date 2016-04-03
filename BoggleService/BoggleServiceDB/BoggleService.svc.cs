using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace BoggleServiceDB
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class BoggleService : IBoggleService
    {
        public Stream API()
        {
            throw new NotImplementedException();
        }

        public void CancelJoin(User user)
        {
            throw new NotImplementedException();
        }

        public UserResponse CreateUser(User user)
        {
            throw new NotImplementedException();
        }

        public BoggleGameResponse GameStatus(string gameID, string brief)
        {
            throw new NotImplementedException();
        }

        public BoggleGameResponse JoinGame(JoinGameRequest requestBody)
        {
            throw new NotImplementedException();
        }

        public BoggleWordResponse PlayWord(string gameID, BoggleWord word)
        {
            throw new NotImplementedException();
        }
    }
}
