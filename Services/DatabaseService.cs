using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Nure.NET.Types;

namespace Api.Services;

public class DatabaseService : IDisposable
{
    private readonly NpgsqlConnection _connection;

    public DatabaseService(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
    }

    private async Task EnsureConnectionOpenAsync()
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }
    }

    public async Task InitializeDatabaseAsync()
    {
        await EnsureConnectionOpenAsync();
        
        var createTablesSql = @"
            CREATE TABLE IF NOT EXISTS subjects (
                id SERIAL PRIMARY KEY,
                title VARCHAR(255),
                brief VARCHAR(50)
            );

            CREATE TABLE IF NOT EXISTS teachers (
                id BIGINT PRIMARY KEY,
                short_name VARCHAR(100),
                full_name VARCHAR(255)
            );

            CREATE TABLE IF NOT EXISTS groups (
                id BIGINT PRIMARY KEY,
                name VARCHAR(100)
            );

            CREATE TABLE IF NOT EXISTS auditories (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100) UNIQUE
            );

            CREATE TABLE IF NOT EXISTS events (
                id BIGSERIAL PRIMARY KEY,
                number_pair INTEGER,
                subject_id INTEGER REFERENCES subjects(id),
                start_time BIGINT,
                end_time BIGINT,
                auditory VARCHAR(100),
                type VARCHAR(10)
            );

            CREATE TABLE IF NOT EXISTS event_teachers (
                event_id BIGINT REFERENCES events(id) ON DELETE CASCADE,
                teacher_id BIGINT REFERENCES teachers(id) ON DELETE CASCADE,
                PRIMARY KEY (event_id, teacher_id)
            );

            CREATE TABLE IF NOT EXISTS event_groups (
                event_id BIGINT REFERENCES events(id) ON DELETE CASCADE,
                group_id BIGINT REFERENCES groups(id) ON DELETE CASCADE,
                PRIMARY KEY (event_id, group_id)
            );";

        await _connection.ExecuteAsync(createTablesSql);
    }

    public async Task AddTeachersAsync(IEnumerable<Teacher> teachers)
    {
        await EnsureConnectionOpenAsync();
        
        var hasRecords = await _connection.ExecuteScalarAsync<bool>("SELECT EXISTS (SELECT 1 FROM teachers)");
        
        if (!hasRecords)
        {
            const string sql = @"
                INSERT INTO teachers (id, short_name, full_name)
                VALUES (@Id, @ShortName, @FullName)";
            await _connection.ExecuteAsync(sql, teachers);
        }
        else
        {
            foreach (var teacher in teachers)
            {
                var existingTeacher = await _connection.QueryFirstOrDefaultAsync<Teacher>(@"
                    SELECT id, short_name, full_name 
                    FROM teachers 
                    WHERE full_name = @FullName OR short_name = @ShortName",
                    new { teacher.FullName, teacher.ShortName });

                if (existingTeacher == null)
                {
                    await _connection.ExecuteAsync(@"
                        INSERT INTO teachers (id, short_name, full_name)
                        VALUES (@Id, @ShortName, @FullName)",
                        teacher);
                }
            }
        }
    }
    
    public async Task AddAuditoriesAsync(IEnumerable<Auditory> auditories)
    {
        await EnsureConnectionOpenAsync();
        
        var hasRecords = await _connection.ExecuteScalarAsync<bool>("SELECT EXISTS (SELECT 1 FROM auditories)");
        
        if (!hasRecords)
        {
            const string sql = @"
                INSERT INTO auditories (name)
                VALUES (@Name)";
            await _connection.ExecuteAsync(sql, auditories);
        }
        else
        {
            foreach (var auditory in auditories)
            {
                var existingAuditory = await _connection.QueryFirstOrDefaultAsync<Auditory>(@"
                    SELECT id, name 
                    FROM auditories 
                    WHERE name = @Name",
                    new { auditory.Name });

                if (existingAuditory == null)
                {
                    await _connection.ExecuteAsync(@"
                        INSERT INTO auditories (name)
                        VALUES (@Name)",
                        auditory);
                }
            }
        }
    }

    public async Task AddGroupsAsync(IEnumerable<Group> groups)
    {
        await EnsureConnectionOpenAsync();
        
        var hasRecords = await _connection.ExecuteScalarAsync<bool>("SELECT EXISTS (SELECT 1 FROM groups)");
        
        if (!hasRecords)
        {
            const string sql = @"
                INSERT INTO groups (id, name)
                VALUES (@Id, @Name)";
            await _connection.ExecuteAsync(sql, groups);
        }
        else
        {
            foreach (var group in groups)
            {
                var existingGroup = await _connection.QueryFirstOrDefaultAsync<Group>(@"
                    SELECT id, name 
                    FROM groups 
                    WHERE name = @Name",
                    new { group.Name });

                if (existingGroup == null)
                {
                    await _connection.ExecuteAsync(@"
                        INSERT INTO groups (id, name)
                        VALUES (@Id, @Name)",
                        group);
                }
            }
        }
    }

    public async Task AddEventsAsync(EventType type, string identifier, IEnumerable<Event> events)
    {
        await EnsureConnectionOpenAsync();
        
        using var transaction = await _connection.BeginTransactionAsync();
        try
        {
            switch (type)
            {
                case EventType.Teacher:
                    var teacherId = long.Parse(identifier);
                    var hasTeacherEvents = await _connection.ExecuteScalarAsync<bool>(
                        "SELECT EXISTS (SELECT 1 FROM event_teachers WHERE teacher_id = @TeacherId)",
                        new { TeacherId = teacherId },
                        transaction);

                    if (hasTeacherEvents)
                    {
                        await _connection.ExecuteAsync(@"
                            DELETE FROM events 
                            WHERE id IN (
                                SELECT event_id 
                                FROM event_teachers 
                                WHERE teacher_id = @TeacherId
                            )",
                            new { TeacherId = teacherId },
                            transaction);
                    }
                    break;

                case EventType.Group:
                    var groupId = long.Parse(identifier);
                    var hasGroupEvents = await _connection.ExecuteScalarAsync<bool>(
                        "SELECT EXISTS (SELECT 1 FROM event_groups WHERE group_id = @GroupId)",
                        new { GroupId = groupId },
                        transaction);

                    if (hasGroupEvents)
                    {
                        await _connection.ExecuteAsync(@"
                            DELETE FROM events 
                            WHERE id IN (
                                SELECT event_id 
                                FROM event_groups 
                                WHERE group_id = @GroupId
                            )",
                            new { GroupId = groupId },
                            transaction);
                    }
                    break;

                case EventType.Auditory:
                    var auditoryName = identifier;
                    
                    var auditoryExists = await _connection.ExecuteScalarAsync<bool>(
                        "SELECT EXISTS (SELECT 1 FROM auditories WHERE name = @Name)",
                        new { Name = auditoryName },
                        transaction);
                        
                    if (!auditoryExists)
                    {
                        throw new InvalidOperationException($"Auditory {auditoryName} does not exist");
                    }
                    
                    var hasAuditoryEvents = await _connection.ExecuteScalarAsync<bool>(
                        "SELECT EXISTS (SELECT 1 FROM events WHERE auditory = @Auditory)",
                        new { Auditory = auditoryName },
                        transaction);

                    if (hasAuditoryEvents)
                    {
                        await _connection.ExecuteAsync(
                            "DELETE FROM events WHERE auditory = @Auditory",
                            new { Auditory = auditoryName },
                            transaction);
                    }
                    break;
            }

            foreach (var evt in events)
            {
                if (evt.Subject != null)
                {
                    if (evt.Subject.Id.HasValue)
                    {
                        var subjectExists = await _connection.ExecuteScalarAsync<bool>(
                            "SELECT EXISTS(SELECT 1 FROM subjects WHERE id = @Id)",
                            new { Id = evt.Subject.Id },
                            transaction);
                            
                        if (!subjectExists)
                        {
                            throw new InvalidOperationException($"Subject with id {evt.Subject.Id} does not exist");
                        }
                    }
                    else
                    {
                        var existingSubject = await _connection.QueryFirstOrDefaultAsync<Subject>(
                            "SELECT id, title, brief FROM subjects WHERE title = @Title AND brief = @Brief",
                            evt.Subject,
                            transaction);

                        if (existingSubject != null)
                        {
                            evt.Subject.Id = existingSubject.Id;
                        }
                        else
                        {
                            evt.Subject.Id = await _connection.ExecuteScalarAsync<int>(@"
                                INSERT INTO subjects (title, brief)
                                VALUES (@Title, @Brief)
                                RETURNING id",
                                evt.Subject,
                                transaction);
                        }
                    }
                }

                var eventId = await _connection.ExecuteScalarAsync<long>(@"
                    INSERT INTO events (number_pair, subject_id, start_time, end_time, auditory, type)
                    VALUES (@NumberPair, @SubjectId, @StartTime, @EndTime, @Auditory, @Type)
                    RETURNING id",
                    new
                    {
                        evt.NumberPair,
                        SubjectId = evt.Subject?.Id,
                        evt.StartTime,
                        evt.EndTime,
                        evt.Auditory,
                        evt.Type
                    },
                    transaction);

                switch (type)
                {
                    case EventType.Teacher:
                        await _connection.ExecuteAsync(
                            "INSERT INTO event_teachers (event_id, teacher_id) VALUES (@EventId, @TeacherId)",
                            new { EventId = eventId, TeacherId = long.Parse(identifier) },
                            transaction);
                        break;

                    case EventType.Group:
                        await _connection.ExecuteAsync(
                            "INSERT INTO event_groups (event_id, group_id) VALUES (@EventId, @GroupId)",
                            new { EventId = eventId, GroupId = long.Parse(identifier) },
                            transaction);
                        break;
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<Group>> GetGroupsAsync()
    {
        await EnsureConnectionOpenAsync();
        return await _connection.QueryAsync<Group>("SELECT id, name FROM groups");
    }

    public async Task<IEnumerable<Teacher>> GetTeachersAsync()
    {
        await EnsureConnectionOpenAsync();
        return await _connection.QueryAsync<Teacher>("SELECT id, short_name, full_name FROM teachers");
    }

    public async Task<IEnumerable<string>> GetAuditoriesAsync()
    {
        await EnsureConnectionOpenAsync();
        return await _connection.QueryAsync<string>("SELECT name FROM auditories");
    }

    public async Task<IEnumerable<Event>> GetEventsByTypeAsync(EventType type, string identifier)
    {
        await EnsureConnectionOpenAsync();
        
        string query;
        object parameters;

        switch (type)
        {
            case EventType.Teacher:
                query = @"
                    SELECT e.id, e.number_pair, e.start_time, e.end_time, e.auditory, e.type,
                           s.id, s.title, s.brief
                    FROM events e
                    LEFT JOIN subjects s ON e.subject_id = s.id
                    INNER JOIN event_teachers et ON e.id = et.event_id
                    WHERE et.teacher_id = @Identifier";
                parameters = new { Identifier = long.Parse(identifier) };
                break;

            case EventType.Group:
                query = @"
                    SELECT e.id, e.number_pair, e.start_time, e.end_time, e.auditory, e.type,
                           s.id, s.title, s.brief
                    FROM events e
                    LEFT JOIN subjects s ON e.subject_id = s.id
                    INNER JOIN event_groups eg ON e.id = eg.event_id
                    WHERE eg.group_id = @Identifier";
                parameters = new { Identifier = long.Parse(identifier) };
                break;

            case EventType.Auditory:
                query = @"
                    SELECT e.id, e.number_pair, e.start_time, e.end_time, e.auditory, e.type,
                           s.id, s.title, s.brief
                    FROM events e
                    LEFT JOIN subjects s ON e.subject_id = s.id
                    WHERE e.auditory = @Identifier";
                parameters = new { Identifier = identifier };
                break;

            default:
                throw new ArgumentException("Invalid event type", nameof(type));
        }

        var events = await _connection.QueryAsync<Event, Subject, Event>(query,
            (evt, subj) =>
            {
                evt.Subject = subj;
                return evt;
            },
            parameters,
            splitOn: "id");

        return events;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}