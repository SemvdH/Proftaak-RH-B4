using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Animation;

namespace Util
{
    public class DataParser
    {
        public const string LOGIN = "LOGIN";
        public const string LOGIN_RESPONSE = "LOGIN RESPONSE";
        public const string START_SESSION = "START SESSION";
        public const string STOP_SESSION = "STOP SESSION";
        public const string SET_RESISTANCE = "SET RESISTANCE";
        public const string NEW_CONNECTION = "NEW CONNECTION";
        public const string DISCONNECT = "DISCONNECT";
        public const string LOGIN_DOCTOR = "LOGIN DOCTOR";
        public const string MESSAGE = "MESSAGE";
        public const string GET_FILE = "GET FILE";
        /// <summary>
        /// makes the json object with LOGIN identifier and username and password
        /// </summary>
        /// <param name="mUsername">username</param>
        /// <param name="mPassword">password</param>
        /// <returns>json object to ASCII to bytes</returns>
        public static byte[] GetLoginJson(string mUsername, string mPassword)
        {
            dynamic json = new
            {
                identifier = LOGIN,
                data = new
                {
                    username = mUsername,
                    password = mPassword,
                }
            };

            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(json));
        }

        internal static string getNameFromBytesBike(byte[] bytes)
        {
            return getName(bytes, 8, bytes.Length - 8);
        }

        /// <summary>
        /// converts the given string parameter into a message using our protocol.
        /// </summary>
        /// <param name="messageToSend">the message string to send</param>
        /// <returns>a byte array using our protocol to send the message</returns>
        public static byte[] GetMessageToSend(string messageToSend)
        {
            dynamic json = new
            {
                identifier = MESSAGE,
                data = new
                {
                    message = messageToSend
                }
            };
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(json));
        }

        internal static string getNameFromBytesBPM(byte[] bytes)
        {
            return getName(bytes, 2, bytes.Length - 2);
        }

        private static string getName(byte[] bytes, int offset, int lenght)
        {
            byte[] nameArray = new byte[lenght];
            Array.Copy(bytes, offset, nameArray, 0, lenght);
            return Encoding.UTF8.GetString(nameArray);
        }

        /// <summary>
        /// creates a message for when the doctor wants to log in.
        /// </summary>
        /// <param name="mUsername">the username of the doctor</param>
        /// <param name="mPassword">the (hashed) password of the doctor</param>
        /// <returns>a byte array using our protocol that contains the username and password of the doctor</returns>
        public static byte[] LoginAsDoctor(string mUsername, string mPassword)
        {
            dynamic json = new
            {
                identifier = LOGIN_DOCTOR,
                data = new
                {
                    username = mUsername,
                    password = mPassword,
                }
            };

            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(json));
        }

        /// <summary>
        /// gets the username and password from a given message array.
        /// </summary>
        /// <param name="jsonbytes">the array of bytes containing the message</param>
        /// <param name="username">the username variable that the username will be put into</param>
        /// <param name="password">the password variable that the password will be put into</param>
        /// <returns><c>true</c> if the username and password were received correctly, <c>false</c> otherwise</returns>
        public static bool GetUsernamePassword(byte[] jsonbytes, out string username, out string password)
        {
            dynamic json = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(jsonbytes));
            try
            {
                username = json.data.username;
                password = json.data.password;
                return true;
            }
            catch
            {
                username = null;
                password = null;
                return false;
            }
        }

        /// <summary>
        /// gets message using our protocol of the given identifier and data.
        /// </summary>
        /// <param name="mIdentifier">the identifier string of the message</param>
        /// <param name="data">the payload data of the message</param>
        /// <returns>a byte array containing the json message with the given parameters, using our protocol.</returns>
        private static byte[] getJsonMessage(string mIdentifier, dynamic data)
        {
            dynamic json = new
            {
                identifier = mIdentifier,
                data
            };
            return getMessage(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(json)), 0x01);
        }

        /// <summary>
        /// gets a message using our protocol with only the given identifier string.
        /// </summary>
        /// <param name="mIdentifier">the identifier to put into the message</param>
        /// <returns>a byte array containing the json with only the identifier, using our protocol.</returns>
        private static byte[] getJsonMessage(string mIdentifier)
        {
            dynamic json = new
            {
                identifier = mIdentifier,
            };
            return getMessage(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(json)), 0x01);
        }

        /// <summary>
        /// gets the login response of the given status
        /// </summary>
        /// <param name="mStatus">the status of the response</param>
        /// <returns>a byte array containing the response for the given status, using our protocol.</returns>
        public static byte[] getLoginResponse(string mStatus)
        {
            return getJsonMessage(LOGIN_RESPONSE, new { status = mStatus });
        }

        /// <summary>
        /// gets the status of the given json message
        /// </summary>
        /// <param name="json">the byte array containing a json message using our protocol</param>
        /// <returns>the response of the message</returns>
        public static string getResponseStatus(byte[] json)
        {
            return ((dynamic)JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json))).data.status;
        }

        /// <summary>
        /// get the identifier from json
        /// </summary>
        /// <param name="bytes">json in ASCII</param>
        /// <param name="identifier">gets the identifier</param>
        /// <returns>if it sucseeded</returns>
        public static bool getJsonIdentifier(byte[] bytes, out string identifier)
        {
            if (bytes.Length <= 5)
            {
                throw new ArgumentException("bytes to short");
            }
            byte messageId = bytes[4];

            if (messageId == 0x01)
            {
                dynamic json = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(bytes.Skip(5).ToArray()));
                identifier = json.identifier;
                return true;
            }
            else
            {
                identifier = "";
                return false;
            }
        }

        /// <summary>
        /// checks if the de message is raw data according to the protocol
        /// </summary>
        /// <param name="bytes">message</param>
        /// <returns>if message contains raw data</returns>
        public static bool isRawDataBikeServer(byte[] bytes)
        {
            if (bytes.Length <= 5)
            {
                throw new ArgumentException("bytes to short");
            }
            return bytes[4] == 0x02;
        }

        public static bool isRawDataBPMServer(byte[] bytes)
        {
            if (bytes.Length <= 5)
            {
                throw new ArgumentException("bytes to short");
            }
            return bytes[4] == 0x03;
        }

        public static bool isRawDataBikeDoctor(byte[] bytes)
        {
            if (bytes.Length <= 5)
            {
                throw new ArgumentException("bytes to short");
            }
            return bytes[4] == 0x04;
        }

        public static bool isRawDataBPMDoctor(byte[] bytes)
        {
            if (bytes.Length <= 5)
            {
                throw new ArgumentException("bytes to short");
            }
            return bytes[4] == 0x05;
        }


        /// <summary>
        /// constructs a message with the payload, messageId and clientId
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="messageId"></param>
        /// <param name="clientId"></param>
        /// <returns>the message ready for sending</returns>
        private static byte[] getMessage(byte[] payload, byte messageId)
        {
            byte[] res = new byte[payload.Length + 5];

            Array.Copy(BitConverter.GetBytes(payload.Length + 5), 0, res, 0, 4);
            res[4] = messageId;
            Array.Copy(payload, 0, res, 5, payload.Length);

            return res;
        }

        /// <summary>
        /// constructs a message with the payload and clientId and assumes the payload is raw data
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="clientId"></param>
        /// <returns>the message ready for sending</returns>
        public static byte[] GetRawBikeDataMessageServer(byte[] payload)
        {
            return getMessage(payload, 0x02);
        }

        public static byte[] GetRawBPMDataMessageServer(byte[] payload)
        {
            return getMessage(payload, 0x03);
        }

        public static byte[] GetRawBikeDataDoctor(byte[] payload, string username)
        {

            return GetRawDataDoctor(payload, username, 0x04);

        }

        public static byte[] GetRawBPMDataDoctor(byte[] payload, string username)
        {
            return GetRawDataDoctor(payload, username, 0x05);
        }

        private static byte[] GetRawDataDoctor(byte[] payload, string username, byte messageID)
        {
            Debug.WriteLine(BitConverter.ToString(Encoding.ASCII.GetBytes(username)));
            byte[] nameArray = Encoding.ASCII.GetBytes(username);
            byte[] total = new byte[nameArray.Length + payload.Length];
            Array.Copy(payload, 0, total, 0, payload.Length);
            Array.Copy(nameArray, 0, total, payload.Length, nameArray.Length);
            return getMessage(total, messageID);

        }
        /// <summary>
        /// constructs a message with the payload and clientId and assumes the payload is json
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="clientId"></param>
        /// <returns>the message ready for sending</returns>
        public static byte[] getJsonMessage(byte[] payload)
        {
            return getMessage(payload, 0x01);
        }

        /// <summary>
        /// gets the message to start a session with the given user username
        /// </summary>
        /// <param name="user">the username of the user we want to start the session for</param>
        /// <returns>a byte array containing the message to start the session of the given user, using our protocol.</returns>
        public static byte[] getStartSessionJson(string user)
        {
            dynamic data = new
            {
                username = user
            };
            return getJsonMessage(START_SESSION, data);
        }

        /// <summary>
        /// gets the message to stop a session with the given user username
        /// </summary>
        /// <param name="user">the username of the user we want to stop the session for</param>
        /// <returns>a byte array containing the message to stop the session of the given user, using our protocol.</returns>
        public static byte[] getStopSessionJson(string user)
        {
            dynamic data = new
            {
                username = user
            };
            return getJsonMessage(STOP_SESSION, data);
        }

        /// <summary>
        /// gets the message to set the resistance of the given user with the given resistance.
        /// </summary>
        /// <param name="user">the username to set the resistance of.</param>
        /// <param name="mResistance">the resistance value to set</param>
        /// <returns>a byte array containing a json messsage to set the user's resistance, using our protocol.</returns>
        public static byte[] getSetResistanceJson(string user, float mResistance)
        {
            dynamic data = new
            {
                username = user,
                resistance = mResistance
            };
            return getJsonMessage(SET_RESISTANCE, data);
        }

        /// <summary>
        /// gets the response message with the given value.
        /// </summary>
        /// <param name="mWorked">the boolean value to indicate if the operation we want to send a response for was successful or not.</param>
        /// <returns>a byte array containing a json message with the response and the given value.</returns>
        public static byte[] getSetResistanceResponseJson(bool mWorked)
        {
            dynamic data = new
            {
                worked = mWorked
            };
            return getJsonMessage(SET_RESISTANCE, data);
        }

        /// <summary>
        /// gets the message to indicate a new connection for the given user.
        /// </summary>
        /// <param name="user">the username of the user to start a connection for.</param>
        /// <returns>a byte array containing a json message to indicate a new connection for the given user, using our protocol.</returns>
        public static byte[] getNewConnectionJson(string user)
        {
            if (user == null)
                throw new ArgumentNullException("user null");
            dynamic data = new
            {
                username = user
            };
            return getJsonMessage(NEW_CONNECTION, data);
        }

        /// <summary>
        /// gets the message for when a user has been disconnected.
        /// </summary>
        /// <param name="user">the username of the user that has been disconnected</param>
        /// <returns>a byte array containing a json message to indicate that the given user has disconnected, using our protocol.</returns>
        public static byte[] getDisconnectJson(string user)
        {
            dynamic data = new
            {
                username = user
            };
            return getJsonMessage(DISCONNECT, data);
        }

        /// <summary>
        /// gets the resistance from the given json message
        /// </summary>
        /// <param name="json">the json messag</param>
        /// <returns>the resistance that was in the message</returns>
        public static float getResistanceFromJson(byte[] json)
        {
            return ((dynamic)JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json))).data.resistance;
        }

        /// <summary>
        /// gets the resistance response from the given json message
        /// </summary>
        /// <param name="json">the byte array containin the json message</param>
        /// <returns>the response of the message, so wether it was successful or not.</returns>
        public static bool getResistanceFromResponseJson(byte[] json)
        {
            Debug.WriteLine("got message " + Encoding.ASCII.GetString(json));
            return ((dynamic)JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json))).data.worked;
        }

        /// <summary>
        /// gets the username from the given response message.
        /// </summary>
        /// <param name="json">the byte array containin the json message</param>
        /// <returns>the username in the message.</returns>
        public static string getUsernameFromResponseJson(byte[] json)
        {
            return ((dynamic)JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json))).data.username;
        }

        /// <summary>
        /// gets the chat message from the given json message.
        /// </summary>
        /// <param name="json">the byte array containin the json message</param>
        /// <returns>the chat message in the json message</returns>
        public static string getChatMessageFromJson(byte[] json)
        {
            return ((dynamic)JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json))).data.chat;
        }

        /// <summary>
        /// gets the username from the given json message.
        /// </summary>
        /// <param name="json">the byte array containin the json message</param>
        /// <returns>the username that is in the message</returns>
        public static string getUsernameFromJson(byte[] json)
        {
            return ((dynamic)JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json))).data.username;
        }

        /// <summary>
        /// gets the byte array with the json message to send a message with the given parameters.
        /// </summary>
        /// <param name="user">the username of the user that wants to send the message</param>
        /// <param name="message">the message the user wants to send</param>
        /// <returns>a byte array containing a json message with the username and corresponding message, using our protocol.</returns>
        public static byte[] getChatJson(string user, string message)
        {
            dynamic data = new
            {
                username = user,
                chat = message
            };
            return getJsonMessage(MESSAGE, data);
        }

        public static byte[] getDataWithoutName(byte[] bytes, int offset, int length)
        {
            byte[] data = new byte[length];
            Array.Copy(bytes, offset, data, 0, length);
            return data;
        }

        public static byte[] GetGetFileMessage(string mUsername, DateTime mDateTime)
        {
            if (mUsername == null)
            {
                throw new ArgumentNullException("username null");
            }
            dynamic data = new
            {
                username = mUsername,
                dateTime = mDateTime.ToString("yyyy-MM-dd HH-mm-ss")
            };
            return getJsonMessage(GET_FILE, data);
        }

        public static string GetUsernameFromGetFileBytes(byte[] json)
        {
            return ((dynamic)JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json))).data.username;
        }

        public static string GetDateFromGetFileBytes(byte[] json)
        {
            return ((string)((dynamic)JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json))).data.dateTime));
        }

        public static byte[] GetFileMessage(byte[] file)
        {
            return getMessage(file, 0x04);
        }
    }
}
