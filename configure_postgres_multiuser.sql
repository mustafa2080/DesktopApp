-- PostgreSQL Configuration for Multi-User Support
-- Run this script as superuser (postgres)

-- 1. Enable connection tracking
ALTER SYSTEM SET track_activities = on;
ALTER SYSTEM SET track_counts = on;
ALTER SYSTEM SET track_io_timing = on;

-- 2. Optimize connection pooling
ALTER SYSTEM SET max_connections = 200;
ALTER SYSTEM SET shared_buffers = '256MB';
ALTER SYSTEM SET effective_cache_size = '1GB';
ALTER SYSTEM SET maintenance_work_mem = '128MB';
ALTER SYSTEM SET work_mem = '4MB';

-- 3. Enable statement timeout (prevent long-running queries)
ALTER SYSTEM SET statement_timeout = '60s';
ALTER SYSTEM SET idle_in_transaction_session_timeout = '60s';

-- 4. Configure locks
ALTER SYSTEM SET max_locks_per_transaction = 256;
ALTER SYSTEM SET deadlock_timeout = '1s';

-- 5. Enable query logging (for debugging)
ALTER SYSTEM SET log_statement = 'mod'; -- Log all modifications
ALTER SYSTEM SET log_duration = on;
ALTER SYSTEM SET log_lock_waits = on;
ALTER SYSTEM SET log_min_duration_statement = 1000; -- Log queries > 1 second

-- 6. Optimize checkpoint and WAL
ALTER SYSTEM SET checkpoint_completion_target = 0.9;
ALTER SYSTEM SET wal_buffers = '16MB';
ALTER SYSTEM SET min_wal_size = '1GB';
ALTER SYSTEM SET max_wal_size = '4GB';

-- 7. Vacuum and autovacuum settings
ALTER SYSTEM SET autovacuum = on;
ALTER SYSTEM SET autovacuum_max_workers = 3;
ALTER SYSTEM SET autovacuum_naptime = '1min';

-- Reload configuration
SELECT pg_reload_conf();

-- 8. Create indexes for better concurrency
\c graceway_accounting

-- Indexes for CashBoxes
CREATE INDEX IF NOT EXISTS idx_cashboxes_isdeleted_isactive 
ON "CashBoxes"(isdeleted, isactive) WHERE NOT isdeleted;

CREATE INDEX IF NOT EXISTS idx_cashboxes_createdby 
ON "CashBoxes"(createdby) WHERE NOT isdeleted;

-- Indexes for CashTransactions
CREATE INDEX IF NOT EXISTS idx_cashtransactions_cashboxid 
ON "CashTransactions"(cashboxid);

CREATE INDEX IF NOT EXISTS idx_cashtransactions_date 
ON "CashTransactions"(transactiondate DESC);

CREATE INDEX IF NOT EXISTS idx_cashtransactions_type 
ON "CashTransactions"(type, transactiondate DESC);

CREATE INDEX IF NOT EXISTS idx_cashtransactions_createdby 
ON "CashTransactions"(createdby);

-- Indexes for Invoices
CREATE INDEX IF NOT EXISTS idx_salesinvoices_date 
ON "SalesInvoices"(invoicedate DESC);

CREATE INDEX IF NOT EXISTS idx_purchaseinvoices_date 
ON "PurchaseInvoices"(invoicedate DESC);

-- Indexes for Users and Permissions
CREATE INDEX IF NOT EXISTS idx_users_roleid 
ON "Users"(roleid) WHERE isactive;

CREATE INDEX IF NOT EXISTS idx_rolepermissions_roleid 
ON "rolepermissions"(roleid);

-- Indexes for AuditLogs
CREATE INDEX IF NOT EXISTS idx_auditlogs_userid_timestamp 
ON "auditlogs"(userid, timestamp DESC);

CREATE INDEX IF NOT EXISTS idx_auditlogs_entitytype_timestamp 
ON "auditlogs"(entitytype, timestamp DESC);

CREATE INDEX IF NOT EXISTS idx_auditlogs_timestamp 
ON "auditlogs"(timestamp DESC);

-- 9. Grant necessary permissions
GRANT CONNECT ON DATABASE graceway_accounting TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;

-- 10. Create monitoring views
CREATE OR REPLACE VIEW active_sessions AS
SELECT 
    pid,
    usename,
    application_name,
    client_addr,
    client_hostname,
    backend_start,
    state_change,
    state,
    query,
    wait_event_type,
    wait_event
FROM pg_stat_activity
WHERE datname = 'graceway_accounting'
AND state <> 'idle'
ORDER BY backend_start DESC;

CREATE OR REPLACE VIEW connection_stats AS
SELECT 
    datname,
    COUNT(*) as total_connections,
    COUNT(*) FILTER (WHERE state = 'active') as active,
    COUNT(*) FILTER (WHERE state = 'idle') as idle,
    COUNT(*) FILTER (WHERE state = 'idle in transaction') as idle_in_transaction
FROM pg_stat_activity
WHERE datname = 'graceway_accounting'
GROUP BY datname;

CREATE OR REPLACE VIEW lock_stats AS
SELECT 
    locktype,
    relation::regclass as table_name,
    mode,
    granted,
    COUNT(*) as lock_count
FROM pg_locks
WHERE database = (SELECT oid FROM pg_database WHERE datname = 'graceway_accounting')
GROUP BY locktype, relation, mode, granted
ORDER BY lock_count DESC;

-- 11. Create function to check for blocking queries
CREATE OR REPLACE FUNCTION check_blocking_queries()
RETURNS TABLE (
    blocked_pid int,
    blocked_user text,
    blocking_pid int,
    blocking_user text,
    blocked_query text,
    blocking_query text
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        blocked.pid AS blocked_pid,
        blocked.usename AS blocked_user,
        blocking.pid AS blocking_pid,
        blocking.usename AS blocking_user,
        blocked.query AS blocked_query,
        blocking.query AS blocking_query
    FROM pg_stat_activity AS blocked
    JOIN pg_locks AS blocked_lock ON blocked.pid = blocked_lock.pid
    JOIN pg_locks AS blocking_lock ON 
        blocked_lock.locktype = blocking_lock.locktype
        AND blocked_lock.database = blocking_lock.database
        AND blocked_lock.relation = blocking_lock.relation
        AND blocked_lock.page = blocking_lock.page
        AND blocked_lock.tuple = blocking_lock.tuple
        AND blocked_lock.virtualxid = blocking_lock.virtualxid
        AND blocked_lock.transactionid = blocking_lock.transactionid
        AND blocked_lock.classid = blocking_lock.classid
        AND blocked_lock.objid = blocking_lock.objid
        AND blocked_lock.objsubid = blocking_lock.objsubid
        AND blocked_lock.pid <> blocking_lock.pid
    JOIN pg_stat_activity AS blocking ON blocking.pid = blocking_lock.pid
    WHERE NOT blocked_lock.granted
    AND blocked.datname = 'graceway_accounting';
END;
$$ LANGUAGE plpgsql;

-- 12. Show current configuration
SELECT 
    name, 
    setting, 
    unit, 
    context 
FROM pg_settings 
WHERE name IN (
    'max_connections',
    'shared_buffers',
    'effective_cache_size',
    'work_mem',
    'maintenance_work_mem',
    'max_locks_per_transaction',
    'statement_timeout',
    'idle_in_transaction_session_timeout',
    'deadlock_timeout'
)
ORDER BY name;

-- 13. Show active sessions
SELECT * FROM active_sessions;

-- 14. Show connection stats
SELECT * FROM connection_stats;

-- Print completion message
DO $$
BEGIN
    RAISE NOTICE '✅ PostgreSQL configuration for multi-user support complete!';
    RAISE NOTICE 'ℹ️ Please restart PostgreSQL service for all changes to take effect';
    RAISE NOTICE 'ℹ️ Use these queries to monitor:';
    RAISE NOTICE '   - SELECT * FROM active_sessions;';
    RAISE NOTICE '   - SELECT * FROM connection_stats;';
    RAISE NOTICE '   - SELECT * FROM lock_stats;';
    RAISE NOTICE '   - SELECT * FROM check_blocking_queries();';
END $$;
