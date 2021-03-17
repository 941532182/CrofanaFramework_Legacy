using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Google.Protobuf;

namespace Crofana.Network
{
    using Core;
    using Cmtp;
    using System.Net;

    /// <summary>
    /// Network manager, manages CMTP communication.
    /// </summary>
    public class NetworkManager
    {

        #region Fields
        private CmtpServer m_server;
        private IDictionary<CmtpOpCode, IController> m_controllerMap;
        private IDictionary<Type, MessageParser> m_parserMap;
        #endregion

        #region Constructors
        public NetworkManager()
        {
            m_server = new(IPAddress.Parse("127.0.0.1"), 1119, 16);
            m_server.OnCmtpMessageReceived += HandleCmtpMessageReceived;
            m_controllerMap = new Dictionary<CmtpOpCode, IController>();
            m_parserMap = new Dictionary<Type, MessageParser>();
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                Type[] types = asm.GetTypes();
                foreach (var type in types)
                {
                    Boolean isController = type.GetInterfaces().Contains(typeof(IController));
                    Boolean isMessage = type.GetInterfaces().Contains(typeof(IMessage));
                    ControllerAttribute attr = type.GetCustomAttribute<ControllerAttribute>();
                    PropertyInfo parserProp = type.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static);
                    object value = parserProp?.GetValue(null);
                    if (isController && attr is not null)
                    {
                        m_controllerMap[attr.OpCode] = Activator.CreateInstance(type) as IController;
                    }
                    if (isMessage && value is not null && value.GetType().IsSubclassOf(typeof(MessageParser)))
                    {
                        m_parserMap[type] = parserProp.GetValue(null) as MessageParser;
                    }
                }
            }
        }
        #endregion

        #region Callbacks
        private void HandleCmtpMessageReceived(SessionHandle handle, CmtpMessage msg)
        {
            if (m_controllerMap.ContainsKey(msg.OpCode))
            {
                IController controller = m_controllerMap[msg.OpCode];
                Type messageType = controller.MessageType;
                if (m_parserMap.ContainsKey(messageType))
                {
                    controller.Response(m_parserMap[messageType].ParseFrom(msg.Body));
                }
            }
        }
        #endregion

        #region Public methods
        public void Start()
        {
            m_server.Start();
        }
        #endregion

    }
}
