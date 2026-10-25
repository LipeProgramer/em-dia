CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE avisos (
        id uuid NOT NULL,
        item_id uuid NOT NULL,
        tipo smallint NOT NULL,
        canal smallint NOT NULL,
        vencimento_referencia date NOT NULL,
        numero smallint NOT NULL,
        enviado_em timestamp with time zone NOT NULL,
        sucesso boolean NOT NULL,
        erro character varying(300),
        CONSTRAINT "PK_avisos" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE categorias (
        id smallint NOT NULL,
        nome character varying(40) NOT NULL,
        icone character varying(40) NOT NULL,
        ordem smallint NOT NULL,
        CONSTRAINT "PK_categorias" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE checkups (
        id uuid NOT NULL,
        usuario_id uuid NOT NULL,
        realizado_em timestamp with time zone NOT NULL,
        respostas jsonb NOT NULL,
        itens_criados smallint NOT NULL,
        CONSTRAINT "PK_checkups" PRIMARY KEY (id),
        CONSTRAINT ck_checkup_itens CHECK (itens_criados >= 0)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE modelos_item (
        id uuid NOT NULL,
        categoria_id smallint NOT NULL,
        nome character varying(80) NOT NULL,
        tipo_vencimento smallint NOT NULL,
        intervalo_dias integer,
        intervalo_km integer,
        garantia_fabricante_meses smallint,
        antecedencia_dias smallint NOT NULL,
        codigo_checkup character varying(30),
        ativo boolean NOT NULL DEFAULT TRUE,
        CONSTRAINT "PK_modelos_item" PRIMARY KEY (id),
        CONSTRAINT ck_modelo_antecedencia CHECK (antecedencia_dias BETWEEN 0 AND 365),
        CONSTRAINT ck_modelo_garantia CHECK (garantia_fabricante_meses IS NULL OR garantia_fabricante_meses > 0),
        CONSTRAINT ck_modelo_intervalo_dias CHECK (intervalo_dias IS NULL OR intervalo_dias > 0),
        CONSTRAINT ck_modelo_intervalo_km CHECK (intervalo_km IS NULL OR intervalo_km > 0)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE refresh_tokens (
        id uuid NOT NULL,
        usuario_id uuid NOT NULL,
        token_hash character varying(128) NOT NULL,
        criado_em timestamp with time zone NOT NULL,
        expira_em timestamp with time zone NOT NULL,
        revogado_em timestamp with time zone,
        CONSTRAINT "PK_refresh_tokens" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE usuarios (
        id uuid NOT NULL,
        nome character varying(100) NOT NULL,
        email character varying(150) NOT NULL,
        senha_hash character varying(100) NOT NULL,
        ultimo_checkup_em timestamp with time zone,
        falhas_login smallint NOT NULL DEFAULT 0,
        bloqueado_ate timestamp with time zone,
        criado_em timestamp with time zone NOT NULL,
        CONSTRAINT "PK_usuarios" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE veiculos (
        id uuid NOT NULL,
        usuario_id uuid NOT NULL,
        apelido character varying(50) NOT NULL,
        tipo smallint NOT NULL,
        km_atual integer NOT NULL,
        media_km_mes integer NOT NULL,
        km_atualizado_em timestamp with time zone NOT NULL,
        criado_em timestamp with time zone NOT NULL,
        CONSTRAINT "PK_veiculos" PRIMARY KEY (id),
        CONSTRAINT ck_veiculo_km CHECK (km_atual >= 0),
        CONSTRAINT ck_veiculo_media_km CHECK (media_km_mes BETWEEN 1 AND 20000)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE passos_modelo (
        id uuid NOT NULL,
        modelo_item_id uuid NOT NULL,
        ordem smallint NOT NULL,
        descricao character varying(120) NOT NULL,
        CONSTRAINT "PK_passos_modelo" PRIMARY KEY (id),
        CONSTRAINT "FK_passos_modelo_modelos_item_modelo_item_id" FOREIGN KEY (modelo_item_id) REFERENCES modelos_item (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE preferencias_aviso (
        usuario_id uuid NOT NULL,
        receber_email boolean NOT NULL DEFAULT TRUE,
        max_reavisos smallint NOT NULL DEFAULT 2,
        intervalo_reaviso_dias smallint NOT NULL DEFAULT 3,
        modo_foco_padrao boolean NOT NULL DEFAULT FALSE,
        atualizado_em timestamp with time zone NOT NULL,
        CONSTRAINT "PK_preferencias_aviso" PRIMARY KEY (usuario_id),
        CONSTRAINT ck_pref_intervalo CHECK (intervalo_reaviso_dias BETWEEN 1 AND 7),
        CONSTRAINT ck_pref_max_reavisos CHECK (max_reavisos BETWEEN 0 AND 5),
        CONSTRAINT "FK_preferencias_aviso_usuarios_usuario_id" FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE itens (
        id uuid NOT NULL,
        usuario_id uuid NOT NULL,
        categoria_id smallint NOT NULL,
        modelo_item_id uuid,
        veiculo_id uuid,
        nome character varying(80) NOT NULL,
        tipo_vencimento smallint NOT NULL,
        data_base date,
        data_vencimento date,
        intervalo_dias integer,
        intervalo_km integer,
        km_base integer,
        garantia_fabricante_meses smallint,
        antecedencia_dias smallint NOT NULL DEFAULT 30,
        proximo_vencimento_data date,
        proximo_vencimento_km integer,
        situacao smallint NOT NULL DEFAULT 1,
        observacao character varying(500),
        criado_em timestamp with time zone NOT NULL,
        atualizado_em timestamp with time zone NOT NULL,
        CONSTRAINT "PK_itens" PRIMARY KEY (id),
        CONSTRAINT ck_item_antecedencia CHECK (antecedencia_dias BETWEEN 0 AND 365),
        CONSTRAINT ck_item_calculado CHECK (tipo_vencimento <> 4 OR (data_base IS NOT NULL AND garantia_fabricante_meses IS NOT NULL)),
        CONSTRAINT ck_item_data_unica CHECK (tipo_vencimento <> 1 OR data_vencimento IS NOT NULL),
        CONSTRAINT ck_item_garantia CHECK (garantia_fabricante_meses IS NULL OR garantia_fabricante_meses > 0),
        CONSTRAINT ck_item_intervalo_dias CHECK (intervalo_dias IS NULL OR intervalo_dias > 0),
        CONSTRAINT ck_item_intervalo_km CHECK (intervalo_km IS NULL OR intervalo_km > 0),
        CONSTRAINT ck_item_km_base CHECK (km_base IS NULL OR km_base >= 0),
        CONSTRAINT ck_item_recorrente CHECK (tipo_vencimento <> 2 OR (data_base IS NOT NULL AND intervalo_dias IS NOT NULL)),
        CONSTRAINT ck_item_uso_tempo CHECK (tipo_vencimento <> 3 OR (veiculo_id IS NOT NULL AND data_base IS NOT NULL AND km_base IS NOT NULL AND intervalo_km IS NOT NULL)),
        CONSTRAINT "FK_itens_modelos_item_modelo_item_id" FOREIGN KEY (modelo_item_id) REFERENCES modelos_item (id) ON DELETE SET NULL,
        CONSTRAINT "FK_itens_veiculos_veiculo_id" FOREIGN KEY (veiculo_id) REFERENCES veiculos (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE historicos (
        id uuid NOT NULL,
        item_id uuid NOT NULL,
        concluido_em date NOT NULL,
        km_na_conclusao integer,
        valor_gasto numeric(10,2),
        vencimento_anterior date,
        proximo_vencimento date,
        observacao character varying(500),
        criado_em timestamp with time zone NOT NULL,
        CONSTRAINT "PK_historicos" PRIMARY KEY (id),
        CONSTRAINT ck_historico_km CHECK (km_na_conclusao IS NULL OR km_na_conclusao >= 0),
        CONSTRAINT ck_historico_valor CHECK (valor_gasto IS NULL OR valor_gasto >= 0),
        CONSTRAINT "FK_historicos_itens_item_id" FOREIGN KEY (item_id) REFERENCES itens (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE TABLE passos (
        id uuid NOT NULL,
        item_id uuid NOT NULL,
        ordem smallint NOT NULL,
        descricao character varying(120) NOT NULL,
        concluido boolean NOT NULL DEFAULT FALSE,
        concluido_em timestamp with time zone,
        CONSTRAINT "PK_passos" PRIMARY KEY (id),
        CONSTRAINT "FK_passos_itens_item_id" FOREIGN KEY (item_id) REFERENCES itens (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    INSERT INTO categorias (id, icone, nome, ordem)
    VALUES (1, 'car', 'Veículo', 1);
    INSERT INTO categorias (id, icone, nome, ordem)
    VALUES (2, 'id-card', 'Documentos', 2);
    INSERT INTO categorias (id, icone, nome, ordem)
    VALUES (3, 'home', 'Casa', 3);
    INSERT INTO categorias (id, icone, nome, ordem)
    VALUES (4, 'heart', 'Saúde', 4);
    INSERT INTO categorias (id, icone, nome, ordem)
    VALUES (5, 'dollar', 'Finanças', 5);
    INSERT INTO categorias (id, icone, nome, ordem)
    VALUES (6, 'shield-check', 'Garantias', 6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    INSERT INTO modelos_item (id, antecedencia_dias, ativo, categoria_id, codigo_checkup, garantia_fabricante_meses, intervalo_dias, intervalo_km, nome, tipo_vencimento)
    VALUES ('a7000000-0000-0000-0000-000000000001', 30, TRUE, 1, 'TEM_CARRO', NULL, 365, 10000, 'Troca de óleo', 3);
    INSERT INTO modelos_item (id, antecedencia_dias, ativo, categoria_id, codigo_checkup, garantia_fabricante_meses, intervalo_dias, intervalo_km, nome, tipo_vencimento)
    VALUES ('a7000000-0000-0000-0000-000000000002', 30, TRUE, 1, 'TEM_CARRO', NULL, 365, NULL, 'IPVA', 2);
    INSERT INTO modelos_item (id, antecedencia_dias, ativo, categoria_id, codigo_checkup, garantia_fabricante_meses, intervalo_dias, intervalo_km, nome, tipo_vencimento)
    VALUES ('a7000000-0000-0000-0000-000000000003', 45, TRUE, 2, 'TEM_CNH', NULL, NULL, NULL, 'CNH', 1);
    INSERT INTO modelos_item (id, antecedencia_dias, ativo, categoria_id, codigo_checkup, garantia_fabricante_meses, intervalo_dias, intervalo_km, nome, tipo_vencimento)
    VALUES ('a7000000-0000-0000-0000-000000000004', 15, TRUE, 3, 'TEM_PURIFICADOR', NULL, 180, NULL, 'Filtro do purificador', 2);
    INSERT INTO modelos_item (id, antecedencia_dias, ativo, categoria_id, codigo_checkup, garantia_fabricante_meses, intervalo_dias, intervalo_km, nome, tipo_vencimento)
    VALUES ('a7000000-0000-0000-0000-000000000005', 30, TRUE, 4, 'TEM_PET', NULL, 365, NULL, 'Vacina do pet', 2);
    INSERT INTO modelos_item (id, antecedencia_dias, ativo, categoria_id, codigo_checkup, garantia_fabricante_meses, intervalo_dias, intervalo_km, nome, tipo_vencimento)
    VALUES ('a7000000-0000-0000-0000-000000000006', 15, TRUE, 6, 'COMPROU_ELETRONICO', 12, NULL, NULL, 'Garantia de eletrônico', 4);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000001', 'Agendar oficina', 'a7000000-0000-0000-0000-000000000001', 1);
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000002', 'Levar o veículo', 'a7000000-0000-0000-0000-000000000001', 2);
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000003', 'Consultar valor', 'a7000000-0000-0000-0000-000000000002', 1);
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000004', 'Pagar', 'a7000000-0000-0000-0000-000000000002', 2);
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000005', 'Agendar exame médico', 'a7000000-0000-0000-0000-000000000003', 1);
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000006', 'Pagar taxa', 'a7000000-0000-0000-0000-000000000003', 2);
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000007', 'Ir ao Detran', 'a7000000-0000-0000-0000-000000000003', 3);
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000008', 'Comprar refil', 'a7000000-0000-0000-0000-000000000004', 1);
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000009', 'Trocar', 'a7000000-0000-0000-0000-000000000004', 2);
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000010', 'Agendar veterinário', 'a7000000-0000-0000-0000-000000000005', 1);
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000011', 'Testar o produto', 'a7000000-0000-0000-0000-000000000006', 1);
    INSERT INTO passos_modelo (id, descricao, modelo_item_id, ordem)
    VALUES ('b1000000-0000-0000-0000-000000000012', 'Separar a nota fiscal', 'a7000000-0000-0000-0000-000000000006', 2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE INDEX ix_avisos_item_vencimento ON avisos (item_id, vencimento_referencia);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_avisos_uk ON avisos (item_id, vencimento_referencia, numero);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_categorias_nome ON categorias (nome);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE INDEX ix_checkups_usuario ON checkups (usuario_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE INDEX ix_historicos_item ON historicos (item_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE INDEX "IX_itens_modelo_item_id" ON itens (modelo_item_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE INDEX ix_itens_radar ON itens (usuario_id, situacao, proximo_vencimento_data);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE INDEX ix_itens_usuario ON itens (usuario_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE INDEX ix_itens_veiculo ON itens (veiculo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE INDEX ix_modelos_categoria ON modelos_item (categoria_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_passos_item_ordem ON passos (item_id, ordem);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_passos_modelo_ordem ON passos_modelo (modelo_item_id, ordem);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_refresh_tokens_hash ON refresh_tokens (token_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE INDEX ix_refresh_tokens_usuario ON refresh_tokens (usuario_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_usuarios_email ON usuarios (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE INDEX ix_veiculos_usuario ON veiculos (usuario_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_veiculos_usuario_apelido ON veiculos (usuario_id, apelido);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025145410_CriacaoInicial') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261025145410_CriacaoInicial', '9.0.4');
    END IF;
END $EF$;
COMMIT;

