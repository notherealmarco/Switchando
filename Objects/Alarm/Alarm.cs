using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation.Objects.Alarm
{
    class Alarm
    {
        public bool Status;
        private uint Pin; 
        public Alarm(uint pin)
        {
            this.Pin = pin;
            this.Status = false;
        }

        public void SwitchOn()
        {
            this.Status = true;
        }
        public bool GetStatus()
        {
            return this.Status;
        }

        public void Unlock(uint pin)
        {
            if (pin == this.Pin)
            {
                this.Status = false;
            }
        }
    }
}
