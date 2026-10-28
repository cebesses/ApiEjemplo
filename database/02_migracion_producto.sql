USE tienda;

ALTER TABLE producto
    CHANGE COLUMN id id_producto INT NOT NULL AUTO_INCREMENT,
    CHANGE COLUMN name nombre VARCHAR(150) NOT NULL,
    CHANGE COLUMN price precio DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    CHANGE COLUMN quantity stock INT NOT NULL DEFAULT 0;

ALTER TABLE producto ADD COLUMN id_categoria INT NULL AFTER nombre;

INSERT IGNORE INTO categoria (nombre) VALUES ('General');

UPDATE producto SET id_categoria = (SELECT MIN(id_categoria) FROM categoria) WHERE id_categoria IS NULL;

ALTER TABLE producto MODIFY COLUMN id_categoria INT NOT NULL;

ALTER TABLE producto
    ADD KEY ix_producto_categoria (id_categoria),
    ADD CONSTRAINT fk_producto_categoria FOREIGN KEY (id_categoria) REFERENCES categoria (id_categoria)
        ON UPDATE CASCADE
        ON DELETE RESTRICT;
