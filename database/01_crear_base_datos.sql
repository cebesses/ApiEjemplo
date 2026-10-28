CREATE DATABASE IF NOT EXISTS tienda
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_spanish_ci;

USE tienda;

CREATE TABLE IF NOT EXISTS categoria (
    id_categoria INT NOT NULL AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL,
    PRIMARY KEY (id_categoria),
    UNIQUE KEY uq_categoria_nombre (nombre)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

CREATE TABLE IF NOT EXISTS cliente (
    id_cliente INT NOT NULL AUTO_INCREMENT,
    nombre VARCHAR(120) NOT NULL,
    correo VARCHAR(150) NOT NULL,
    ciudad VARCHAR(100) NULL,
    fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_cliente),
    UNIQUE KEY uq_cliente_correo (correo)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

CREATE TABLE IF NOT EXISTS producto (
    id_producto INT NOT NULL AUTO_INCREMENT,
    nombre VARCHAR(150) NOT NULL,
    id_categoria INT NOT NULL,
    precio DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    stock INT NOT NULL DEFAULT 0,
    PRIMARY KEY (id_producto),
    KEY ix_producto_categoria (id_categoria),
    CONSTRAINT fk_producto_categoria FOREIGN KEY (id_categoria) REFERENCES categoria (id_categoria)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;
