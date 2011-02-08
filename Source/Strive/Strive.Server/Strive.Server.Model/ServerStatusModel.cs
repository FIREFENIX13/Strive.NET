﻿using System;
using UpdateControls;


namespace Strive.Server.Model
{
    public class ServerStatusModel
    {
        private string _status;
        private DateTime _started;

        #region Independent properties
        // Generated by Update Controls --------------------------------
        private Independent _indStatus = new Independent();
        private Independent _indStarted = new Independent();

        public string Status
        {
            get { _indStatus.OnGet(); return _status; }
            set { _indStatus.OnSet(); _status = value; }
        }

        public DateTime Started
        {
            get { _indStarted.OnGet(); return _started; }
            set { _indStarted.OnSet(); _started = value; }
        }
        // End generated code --------------------------------
        #endregion
    }
}