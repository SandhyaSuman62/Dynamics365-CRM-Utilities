        ///Added TotalRecords<T> utility to fetch CRM records with paging (5000+ records support)
public T TotalRecords<T>(string entityName, FilterExpression filter = null)
        {
            string fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                            <entity name='{entityName}'>
                                <attribute name='{entityName}id'/>";

            if (filter != null && filter.Conditions.Count > 0)
            {
                fetchXml += "<filter type='" + (filter.FilterOperator == LogicalOperator.And ? "and" : "or") + "'>";

                foreach (ConditionExpression condition in filter.Conditions)
                {
                    string operatorString = GetFetchXmlOperator(condition.Operator);
                    fetchXml += $"<condition attribute='{condition.AttributeName}' operator='{operatorString}' value='{condition.Values[0]}'/>";
                }

                fetchXml += "</filter>";
            }

            fetchXml += "</entity></fetch>";

            List<Entity> entities = FetchRetrieveAll(new FetchExpression(fetchXml));
            if (typeof(T) == typeof(List<Entity>))
            {
                return (T)(object)entities;
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)entities.Count;
            }
            else
            {
                throw new InvalidOperationException("Invalid type specified for TotalRecords method");
            }
        }

        ///  FetchXML to QueryExpression and retrieves all records.
        public List<Entity> FetchRetrieveAll(FetchExpression query)
        {

            var conversionRequest = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = query.Query
            };

            FetchXmlToQueryExpressionResponse conversionResponse = (FetchXmlToQueryExpressionResponse)service.Execute(conversionRequest);

            return RetrieveAll(conversionResponse.Query);
        }

        /// Retrieves all records for a given QueryExpression with paging support (5000 + records).
        public List<Entity> RetrieveAll(QueryExpression query)
        {
            var result = new List<Entity>();

            EntityCollection entities = service.RetrieveMultiple(query);
            result.AddRange(entities.Entities);

            var page = 2;
            while (entities.MoreRecords)
            {
                query.PageInfo = new PagingInfo
                {
                    PagingCookie = entities.PagingCookie,
                    PageNumber = page
                };

                entities = service.RetrieveMultiple(query);
                result.AddRange(entities.Entities);
                page++;
            }

            return result;
        }

       /// Maps CRM ConditionOperator to FetchXML operator string.
      /// Extend this method to support additional operators as needed.

        private string GetFetchXmlOperator(ConditionOperator conditionOperator)
        {
            switch (conditionOperator)
            {
                case ConditionOperator.Equal:
                    return "eq";
                case ConditionOperator.NotEqual:
                    return "ne";
                case ConditionOperator.GreaterThan:
                    return "gt";
                case ConditionOperator.LessThan:
                    return "lt";
                default:
                    throw new NotImplementedException("Operator not implemented");
            }
        }
