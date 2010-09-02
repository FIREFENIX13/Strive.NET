﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpdateControls;

namespace Strive.Server.WPF.Model
{
    public class LogModel
    {
        private List<string> _logEntries = new List<string>();

        #region Independent properties
        // Generated by Update Controls --------------------------------
        private Independent _indLogEntries = new Independent();

        public void NewLogEntry(string logEntry)
        {
            _indLogEntries.OnSet();
            _logEntries.Insert(0, logEntry);
        }

        public void TruncateLogs(int length)
        {
            _indLogEntries.OnSet();
            while (_logEntries.Count > length)
                _logEntries.RemoveAt(length);
        }

        public IEnumerable<string> LogEntries
        {
            get { _indLogEntries.OnGet(); return _logEntries; }
        }
        // End generated code --------------------------------
        #endregion
    }
}