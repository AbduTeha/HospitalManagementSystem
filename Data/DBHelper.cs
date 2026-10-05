using System.Data;
using Microsoft.Data.SqlClient;
using HospitalManagementSystem.Core.Exceptions;

namespace HospitalManagementSystem.Data;

public sealed class DBHelper
{
    public DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
    {
        try
        {
            using var connection = DBConnection.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            AddParameters(command, parameters);
            using var adapter = new SqlDataAdapter(command);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }
        catch (SqlException ex)
        {
            throw new DatabaseExecutionException("The database query could not be completed.", ex);
        }
    }

    public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
    {
        try
        {
            using var connection = DBConnection.CreateConnection();
            connection.Open();
            using var command = new SqlCommand(sql, connection);
            AddParameters(command, parameters);
            return command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw new DatabaseExecutionException("The database operation could not be completed.", ex);
        }
    }

    public object? ExecuteScalar(string sql, params SqlParameter[] parameters)
    {
        try
        {
            using var connection = DBConnection.CreateConnection();
            connection.Open();
            using var command = new SqlCommand(sql, connection);
            AddParameters(command, parameters);
            return command.ExecuteScalar();
        }
        catch (SqlException ex)
        {
            throw new DatabaseExecutionException("The database operation could not be completed.", ex);
        }
    }

    public T ExecuteTransaction<T>(Func<SqlConnection, SqlTransaction, T> operation)
    {
        try
        {
            using var connection = DBConnection.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var result = operation(connection, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                try { transaction.Rollback(); } catch { }
                throw;
            }
        }
        catch (DatabaseExecutionException)
        {
            throw;
        }
        catch (SqlException ex)
        {
            throw new DatabaseExecutionException("The database transaction could not be completed.", ex);
        }
    }

    public void ExecuteTransaction(Action<SqlConnection, SqlTransaction> operation) =>
        ExecuteTransaction<object?>( (connection, transaction) => { operation(connection, transaction); return null; } );

    public static void AddParameters(SqlCommand command, IEnumerable<SqlParameter> parameters)
    {
        foreach (var parameter in parameters)
            command.Parameters.Add(parameter);
    }
}
