﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ManyWho.Flow.SDK.Errors;
using ManyWho.Flow.SDK.Security;
using Newtonsoft.Json;
using System.Net.Http;

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

namespace ManyWho.Flow.SDK.Utils
{
    public class ErrorUtils
    {
        public static void SendAlert(INotifier notifier, IAuthenticatedWho authenticatedWho, String alertType, String alertMessage)
        {
            SendAlert(notifier, authenticatedWho, alertType, alertMessage, null);
        }

        public static void SendAlert(INotifier notifier, IAuthenticatedWho authenticatedWho, String alertType, String alertMessage, Exception exception)
        {
            String message = null;

            // Check to see if the caller has in fact provided a notifier - if not, we don't bother to do anything
            if (notifier != null)
            {
                try
                {
                    // Create the full message
                    message = "";

                    // Create the alert message block
                    message += "Alert Message" + Environment.NewLine;
                    message += "-----" + Environment.NewLine;
                    message += alertMessage + Environment.NewLine + Environment.NewLine;

                    // Only include the authenticated who if we have one
                    if (authenticatedWho != null)
                    {
                        // Create the running user summary block
                        message += "Affected User" + Environment.NewLine;
                        message += "-------------" + Environment.NewLine;

                        // Serialize the user information
                        message += NotificationUtils.SerializeAuthenticatedWhoInfo(NotificationUtils.MEDIA_TYPE_PLAIN, authenticatedWho) + Environment.NewLine;
                    }

                    // Finally, we add the exception details if there is an exception
                    message += AggregateAndWriteErrorMessage(exception, "", true);

                    // Set the notification and send
                    notifier.AddNotificationMessage(NotificationUtils.MEDIA_TYPE_PLAIN, message);
                    notifier.SendNotification();
                }
                catch (Exception)
                {
                    // Hide any faults so we're not piling errors on errors
                }
            }
        }

        private static String AggregateAndWriteErrorMessage(Exception exception, String message, Boolean includeTrace)
        {
            if (exception != null)
            {
                if (exception is AggregateException)
                {
                    message = AggregateAndWriteAggregateErrorMessage((AggregateException)exception, message, includeTrace);
                }
                else
                {
                    message = AggregateAndWriteExceptionErrorMessage(exception, message, includeTrace);
                }
            }

            return message;
        }

        private static String AggregateAndWriteAggregateErrorMessage(Exception exception, String message, Boolean includeTrace)
        {
            if (exception is AggregateException)
            {
                AggregateException aex = (AggregateException)exception;

                message += "The exception is an aggregate of the following exceptions:" + Environment.NewLine + Environment.NewLine;

                if (aex.InnerExceptions != null &&
                    aex.InnerExceptions.Any())
                {
                    foreach (Exception innerException in aex.InnerExceptions)
                    {
                        if (innerException is AggregateException)
                        {
                            message = AggregateAndWriteAggregateErrorMessage((AggregateException)innerException, message, includeTrace);
                        }
                        else
                        {
                            message = AggregateAndWriteErrorMessage(innerException, message, includeTrace);
                        }
                    }
                }
            }

            return message;
        }

        private static String AggregateAndWriteExceptionErrorMessage(Exception exception, String message, Boolean includeTrace)
        {
            if (exception != null)
            {
                message += "Exception:" + Environment.NewLine;
                message += exception.Message + Environment.NewLine + Environment.NewLine;

                if (includeTrace == true)
                {
                    message += "Stack Trace:" + Environment.NewLine;
                    message += exception.StackTrace + Environment.NewLine + Environment.NewLine;
                }
            }

            return message;
        }

        public static async Task<ApiProblemException> BuildProblemException(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.Headers.Contains("X-ManyWho-Service-Problem-Kind") || string.IsNullOrWhiteSpace(responseBody))
            {
                return new ServiceProblemException(new ServiceProblem(response.RequestMessage.RequestUri.AbsoluteUri, response, responseBody));
            }

            var values = response.Headers.GetValues("X-ManyWho-Service-Problem-Kind");
            if (!values.Any())
            {
                return new ServiceProblemException(new ServiceProblem(response.RequestMessage.RequestUri.AbsoluteUri, response, responseBody));
            }

            var problemKind = (ProblemKind)Enum.Parse(typeof(ProblemKind), values.FirstOrDefault());
            switch (problemKind)
            {
                case ProblemKind.api:
                    return new ApiProblemException(JsonConvert.DeserializeObject<ApiProblem>(responseBody));
                case ProblemKind.service:
                    return new ServiceProblemException(JsonConvert.DeserializeObject<ServiceProblem>(responseBody));
            }

            return new ServiceProblemException(new ServiceProblem(response.RequestMessage.RequestUri.AbsoluteUri, response, responseBody));
        }
    }
}
