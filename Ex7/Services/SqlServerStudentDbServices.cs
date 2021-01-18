using Ex7.DTOs.Requests;
using Ex7.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ex7.Services
{
    public class SqlServerStudentDbServices : IStudentDbService
    {
        private SqlConnection sqlcon = new SqlConnection("Data Source=db-mssql; Initial Catalog=s7452; Integrated Security=True");

        public Student GetStudent(string index)
        {
            using (var con = sqlcon)
            using (var com = new SqlCommand())
            {

                com.Connection = con;
                com.CommandText = $"select * from Student where indexNumber=@id";
                com.Parameters.AddWithValue("id", index);
                con.Open();
                var dr = com.ExecuteReader();
                var st = new Student();
                while (dr.Read())
                {
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.Password = dr["password"].ToString();
                }
                return st;
            }
        }


        public IEnumerable<Student> GetStudents()
        {
            using (var con = sqlcon)
            using (var com = new SqlCommand())
            {

                var _students = new List<Student>();
                com.Connection = con;
                com.CommandText = "select * from Student";

                con.Open();
                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var st = new Student();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.Password = dr["password"].ToString();
                    _students.Add(st);
                }
                return _students;
            }
        }

        public void EnrollStudent(EnrollStudentRequest request)
        {
            var st = new Student();
            st.IndexNumber = request.IndexNumber;
            st.FirstName = request.FirstName;
            st.LastName = request.LastName;
            st.BirthDate = request.BirthDate;

            using (var con = sqlcon)
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                var tran = con.BeginTransaction();
                com.Transaction = tran;
                try
                {

                    // 1. Czy studia istnieją?
                    com.CommandText = "SELECT IdStudy FROM studies WHERE name=@name";
                    com.Parameters.AddWithValue("name", request.Studies);

                    var dr = com.ExecuteReader();
                    if (!dr.Read())
                    {
                        tran.Rollback();
                    }

                    int idStudies = (int)dr["IdStudy"];
                    dr.Close();

                    // 2. Czy enrollment istnieje? Jeżeli nie to stworzenie nowego
                    com.CommandText = "SELECT IdEnrollment FROM enrollment WHERE idStudy=@idStudy AND Semester=1";
                    com.Parameters.AddWithValue("idStudy", idStudies);

                    int idEnrollment = 1;
                    dr = com.ExecuteReader();
                    if (!dr.Read())
                    {
                        dr.Close();
                        com.CommandText = "INSERT INTO Enrollment(idEnrollment, Semester, IdStudy, StartDate) VALUES((select (max(idEnrollment)+1) from enrollment),1 , @idStudies, @startDate)";
                        com.Parameters.AddWithValue("idStudies", idStudies);
                        com.Parameters.AddWithValue("startDate", DateTime.Now);
                        dr = com.ExecuteReader();
                        dr.Close();

                        com.CommandText = "SELECT IdEnrollment FROM enrollment WHERE idStudy=@idStudy AND Semester=1";
                        dr = com.ExecuteReader();
                        if (dr.Read())
                        {
                            idEnrollment = (int)dr["idEnrollment"];
                        }
                    }
                    else
                    {
                        idEnrollment = (int)dr["idEnrollment"];
                    }
                    dr.Close();

                    // 3. Sprawdzanie studenta czy jest unikalny
                    com.CommandText = "SELECT IndexNumber FROM student WHERE IndexNumber=@indexNumber";
                    com.Parameters.AddWithValue("indexNumber", st.IndexNumber);

                    dr = com.ExecuteReader();
                    if (dr.Read())
                    {
                        tran.Rollback();
                    }
                    dr.Close();
                    // 4. Dodanie studenta

                    com.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, idEnrollment) VALUES(@index, @fname, @lname, @birthdate, @idEnrollment)";
                    com.Parameters.AddWithValue("index", st.IndexNumber);
                    com.Parameters.AddWithValue("fname", st.FirstName);
                    com.Parameters.AddWithValue("lname", st.LastName);
                    com.Parameters.AddWithValue("birthdate", st.BirthDate);
                    com.Parameters.AddWithValue("idEnrollment", idEnrollment);

                    dr = com.ExecuteReader();
                    dr.Close();
                    tran.Commit();
                }
                catch (SqlException exc)
                {
                    tran.Rollback();
                }

            }
        }

        public void PromoteStudnet(int semester, string studies)
        {
            /*CREATE PROCEDURE PromoteStudents @Studies NVARCHAR(100), @Semester INT
            AS
            BEGIN
                SET XACT_ABORT ON;
                BEGIN TRAN

                DECLARE @IdStudies INT = (SELECT IdStudies FROM Studies WHERE Name = @Studies);
                IF @IdStudies IS NULL
                BEGIN
                    RETURN;
                END

                DECLARE @IdEnrollment INT; *//*SELECT*//*

            
                IF @IdEnrollment IS NULL
                BEGIN
                    INSERT 
                END


                UPDATE Students SELECT
                IdEnrollment = @IdEnrollment
                WHERE

                COMMIT
            
            END;*/

            using (var con = new SqlConnection(""))
            using (var com = new SqlCommand("PromoteStudents", con)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                con.Open();
                com.ExecuteNonQuery();
            }
        }
    }
}
