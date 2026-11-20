-- Script de creación de la base de datos de Mesa de Ayuda
CREATE DATABASE IF NOT EXISTS mesa_ayuda
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE mesa_ayuda;

CREATE TABLE IF NOT EXISTS tickets (
    id INT NOT NULL AUTO_INCREMENT,
    titulo VARCHAR(80) NOT NULL,
    descripcion VARCHAR(500) NOT NULL,
    solicitante VARCHAR(60) NOT NULL,
    prioridad ENUM('Baja','Media','Alta') NOT NULL DEFAULT 'Media',
    estado ENUM('Abierto','EnProceso','Resuelto','Cerrado') NOT NULL DEFAULT 'Abierto',
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    INDEX ix_tickets_estado (estado),
    INDEX ix_tickets_fecha (fecha_creacion)
) ENGINE=InnoDB;

-- Registros opcionales para realizar la demostración inicial.
INSERT INTO tickets (titulo, descripcion, solicitante, prioridad, estado)
SELECT 'Lector de códigos no funciona', 'El lector no reconoce algunos productos en la caja.', 'Cajero de muestra', 'Media', 'Abierto'
WHERE NOT EXISTS (SELECT 1 FROM tickets);
