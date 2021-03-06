﻿using System;
using System.Runtime.Serialization;

/*!

Copyright 2013 Manywho, Inc.

Licensed under the Manywho License, Version 1.0 (the "License"); you may not use this
file except in compliance with the License.

You may obtain a copy of the License at: http://manywho.com/sharedsource

Unless required by applicable law or agreed to in writing, software distributed under
the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.

*/

namespace ManyWho.Flow.SDK.Run.Elements.Map
{
    [DataContract(Namespace = "http://www.manywho.com/api")]
    public class OutcomeAvailableAPI : IComparable
    {
        [DataMember]
        public String id
        {
            get;
            set;
        }

        [DataMember]
        public String developerName
        {
            get;
            set;
        }

        [DataMember]
        public String label
        {
            get;
            set;
        }

        [DataMember]
        public Int32 order
        {
            get;
            set;
        }

        public int CompareTo(Object obj)
        {
            OutcomeAvailableAPI outcome;
            int result = 0;

            if (obj is OutcomeAvailableAPI)
            {
                outcome = (OutcomeAvailableAPI)obj;

                if (outcome.order > this.order)
                {
                    result = -1;
                }
                else if (outcome.order < this.order)
                {
                    result = 1;
                }
            }

            return result;
        }
    }
}
