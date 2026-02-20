-- Migration : Ajout des champs financiers pour l'extraction triple hybride
-- Date : 20 Février 2026

ALTER TABLE ocr.ExtractedInvoices 
ADD COLUMN VendorSiret TEXT,
ADD COLUMN CustomerSiret TEXT,
ADD COLUMN VendorVatNumber TEXT,
ADD COLUMN ConfidenceScore INT DEFAULT 0,
ADD COLUMN DueDate DATE;

COMMENT ON COLUMN ocr.ExtractedInvoices.VendorSiret IS 'SIRET du fournisseur (14 chiffres)';
COMMENT ON COLUMN ocr.ExtractedInvoices.CustomerSiret IS 'SIRET du destinataire (14 chiffres)';
COMMENT ON COLUMN ocr.ExtractedInvoices.VendorVatNumber IS 'Numéro de TVA intracommunautaire';
COMMENT ON COLUMN ocr.ExtractedInvoices.ConfidenceScore IS 'Score de confiance de l''extraction hybride (0-100)';
COMMENT ON COLUMN ocr.ExtractedInvoices.DueDate IS 'Date d''échéance de la facture';
