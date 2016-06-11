namespace ExpressionToSql
{
    using System.Text;

    internal class QueryBuilder
    {
        private readonly StringBuilder _sb;
        private bool _firstCondition = true;
        private const string AliasName = "a";

        public QueryBuilder() : this(new StringBuilder())
        {
        }

        public QueryBuilder(StringBuilder sb)
        {
            if (sb.Length == 0)
            {
                sb.Append("SELECT");
            }
            _sb = sb;
        }

        public QueryBuilder Take(int count)
        {
            _sb.Append(" TOP ").Append(count);
            return this;
        }

        public QueryBuilder AddParameter(string parameterName)
        {
            _sb.Append(" @").Append(parameterName);
            return this;
        }

        public QueryBuilder AddAttribute(string attributeName, string aliasName = AliasName)
        {
            _sb.Append(" ");

            if (!string.IsNullOrWhiteSpace(aliasName))
            {
                _sb.Append(aliasName).Append(".");
            }

            AppendEscapedValue(attributeName);
            return this;
        }

        public QueryBuilder AddValue(object value)
        {
            _sb.Append(" ").Append(value);
            return this;
        }

        public QueryBuilder AddSeparator()
        {
            _sb.Append(",");
            return this;
        }

        public QueryBuilder Remove(int count = 1)
        {
            _sb.Length -= count;
            return this;
        }

        public QueryBuilder AddTable<T>(Table<T> table, string aliasName = AliasName)
        {
            _sb.Append(" FROM ");

            if (!string.IsNullOrWhiteSpace(table.Schema))
            {
                AppendEscapedValue(table.Schema);
                _sb.Append(".");
            }

            AppendEscapedValue(table.Name);

            if (!string.IsNullOrWhiteSpace(aliasName))
            {
                _sb.Append(" AS ").Append(aliasName);
            }
            return this;
        }

        public QueryBuilder AddCondition(Operand operand, string attributeName, string parameterName, string aliasName = AliasName)
        {
            AppendAndCondition(operand, attributeName, aliasName);
            AddParameter(parameterName);
            return this;
        }

        public QueryBuilder AddCondition(Operand operand, string attributeName, object value, string aliasName = AliasName)
        {
            AppendAndCondition(operand, attributeName, aliasName);
            AddValue(value);
            return this;
        }

        public QueryBuilder OrCondition(Operand operand, string attributeName, string parameterName, string aliasName = AliasName)
        {
            AppendOrCondition(operand, attributeName, aliasName);
            AddParameter(parameterName);
            return this;
        }

        public QueryBuilder OrCondition(Operand operand, string attributeName, object value, string aliasName = AliasName)
        {
            AppendOrCondition(operand, attributeName, aliasName);
            AddValue(value);
            return this;
        }

        private void AppendAndCondition(Operand operand, string attributeName, string aliasName)
        {
            AppendCondition(operand, attributeName, aliasName, "AND");
        }

        private void AppendOrCondition(Operand operand, string attributeName, string aliasName)
        {
            AppendCondition(operand, attributeName, aliasName, "OR");
        }

        private void AppendCondition(Operand operand, string attributeName, string aliasName, string condition)
        {
            AppendCondition(condition);
            AddAttribute(attributeName, aliasName);
            _sb.Append(" ").Append(operand.ToSqlOperand());
        }

        private void AppendCondition(string condition)
        {
            if (_firstCondition)
            {
                _firstCondition = false;
                condition = "WHERE";
            }
            _sb.Append(" ").Append(condition);
        }

        private void AppendEscapedValue(string attributeName)
        {
            if (attributeName.StartsWith("[") && attributeName.EndsWith("]"))
            {
                _sb.Append(attributeName);
                return;
            }
            _sb.Append("[").Append(attributeName).Append("]");
        }
    }
}