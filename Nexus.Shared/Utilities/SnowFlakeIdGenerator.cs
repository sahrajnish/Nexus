using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Shared.Utilities
{
    public class SnowFlakeIdGenerator
    {
        private const long CustonEpoch = 1735689600000L; // January 1, 2025

        // Bit Allocations
        private const int MachineIdBits = 10;
        private const int SequenceBits = 12;

        //Max Values
        private const long MaxMachineId = -1L ^ (-1L << MachineIdBits); // We can simply write 1023 here but to prevent future errors on change of MachineBits we are doing this.
        private const long MaxSequenceBits = -1L ^ (-1L << SequenceBits);

        // Shift Values
        private const int MachineIdShift = SequenceBits;
        private const int TimestampShift = SequenceBits + MachineIdBits;

        // Id of the machine
        private readonly long _machineId;

        //variable to track last timestamp and sequence
        private long _lastTimestamp = -1L;
        private long _sequence = 0L;

        // For thread safety. Multiple users may try to generate Ids at the same time.
        private readonly object _lock = new object();

        public SnowFlakeIdGenerator(long machineId)
        {
            if(machineId > MaxMachineId || machineId < 0)
            {
                throw new ArgumentException($"Machine ID must be between 0 and {MaxMachineId}");
            }
            _machineId = machineId;
        }

        public long NextId()
        {
            // Thread Safety. Allow one user at a time.
            lock(_lock)
            {
                // Current Time
                long CurrentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                // To Prevent Backward clocks - clock drift protection.
                if(CurrentTimestamp < _lastTimestamp)
                {
                    throw new Exception("Clock moves backward. Refusing to generate ID");
                }

                // Same MilliSecond handling
                if (_lastTimestamp == CurrentTimestamp)
                {
                    // Increase sequence and '& MaxSequenceBits' ensures that if we reach 4096 then it wraps to 0.
                    _sequence = (_sequence + 1) & MaxSequenceBits;

                    // If sequence is ). then we have exhaushed all sequence for that millis.
                    // so wait for next millis.
                    if (_sequence == 0)
                    {
                        CurrentTimestamp = WaitNextMillis(_lastTimestamp);
                    }
                } else
                {
                    // new millis. So reset sequence to 0.
                    _sequence = 0;
                }

                // Save state
                _lastTimestamp = CurrentTimestamp;

                return ((CurrentTimestamp - CustonEpoch) << TimestampShift) |
                    (_machineId << MachineIdShift) | _sequence;
            }
        }

        // Helper : Spins until next millis.
        private long WaitNextMillis(long lastTimestamp)
        {
            long TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while(TimeStamp <= lastTimestamp)
            {
                TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            return TimeStamp;
        }
    }
}
