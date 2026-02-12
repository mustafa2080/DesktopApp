-- Check Admin User
SELECT u.userid, u.username, u.fullname, r.rolename 
FROM users u 
LEFT JOIN roles r ON u.roleid = r.roleid 
WHERE u.username = 'admin';

-- Check Admin Permissions Count
SELECT r.rolename, COUNT(rp.permissionid) as permission_count
FROM roles r
LEFT JOIN rolepermissions rp ON r.roleid = rp.roleid
WHERE r.rolename = 'Administrator'
GROUP BY r.rolename;

-- Check All Modules for Admin
SELECT DISTINCT p.module
FROM permissions p
INNER JOIN rolepermissions rp ON p.permissionid = rp.permissionid
INNER JOIN roles r ON rp.roleid = r.roleid
WHERE r.rolename = 'Administrator'
ORDER BY p.module;

-- Count permissions per module for Admin
SELECT p.module, COUNT(*) as permission_count
FROM permissions p
INNER JOIN rolepermissions rp ON p.permissionid = rp.permissionid
INNER JOIN roles r ON rp.roleid = r.roleid
WHERE r.rolename = 'Administrator'
GROUP BY p.module
ORDER BY p.module;
