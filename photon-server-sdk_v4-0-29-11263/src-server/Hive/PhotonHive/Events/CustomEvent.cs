// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomEvent.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Implementation of a custom event.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Hive.Events
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Photon.Hive.Operations;
    using Photon.SocketServer.Rpc;

    /// <summary>
    /// Implementation of a custom event.
    /// </summary>
    [Serializable]
    public class CustomEvent : HiveEventBase
    {
        internal int frame
        {
            get
            {
                var t = Data as Hashtable;
                if (t!=null)
                    return (int) t[(byte) 2];
                
                return 0;
            }
        }
        
        internal int? method
        {
            get
            {
                var t = Data as Hashtable;
                if (t!=null)
                    return (int) t[(byte) 5];
                
                return null;
            }
        }
        
        internal int viewId
        {
            get
            {
                var t = Data as Hashtable;
                if (t!=null)
                    return (int) t[(byte) 0];
                
                return 0;
            }
        }
        
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEvent"/> class.
        /// </summary>
        /// <param name="actorNr">
        /// The actor nr.
        /// </param>
        /// <param name="eventCode">
        /// The event code.
        /// </param>
        /// <param name="data">
        /// The event data.
        /// </param>
        public CustomEvent(int actorNr, byte eventCode, object data)
            : base(actorNr)
        {
            this.Code = eventCode;
            this.Data = data;
        }

        internal CustomEvent(IList<object> list)
            : base(Convert.ToInt32(list[0]))
        {
            this.Code = Convert.ToByte(list[1]);
            this.Data = list[2];
        }

        /// <summary>
        /// Gets or sets the event data.
        /// </summary>
        /// <value>The event data.</value>
        [DataMember(Code = (byte)ParameterKey.Data, IsOptional = true)]
        public object Data { get; set; }


        internal IList<object> AsList()
        {
            return new object[]
                           {
                               this.ActorNr,
                               this.Code,
                               this.Data
                           };
        }
    }
}