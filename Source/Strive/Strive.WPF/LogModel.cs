﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

using UpdateControls;

namespace Strive.WPF
{
    public class LogModel : TraceListener
    {
        private List<string> _logEntries = new List<string>();
        private int _maxEntries = 200;

        public LogModel() : this(true) { }

        public LogModel(bool trace)
        {
            if (trace)
                Trace.Listeners.Add(this);
        }

        private string messageSoFar = String.Empty;
        public override void Write(string message)
        {
            messageSoFar += message;
        }

        public override void WriteLine(string message)
        {
            NewLogEntry(messageSoFar + message);
            messageSoFar = String.Empty;
        }

        #region Independent properties
        // Generated by Update Controls --------------------------------
        private Independent _indMaxEntries = new Independent();
        private Independent _indLogEntries = new Independent();

        public void NewLogEntry(string logEntry)
        {
            _indLogEntries.OnSet();
            _logEntries.Insert(0, logEntry);
            while (_logEntries.Count > _maxEntries)
                _logEntries.RemoveAt(_maxEntries);
        }

        public IEnumerable<string> LogEntries
        {
            get { _indLogEntries.OnGet(); return _logEntries; }
        }

        public int MaxEntries
        {
            get { _indMaxEntries.OnGet(); return _maxEntries; }
            set { _indMaxEntries.OnSet(); _maxEntries = value; }
        }
        // End generated code --------------------------------
        #endregion
    }
}
