/**
 * Types et interfaces pour la gestion documentaire et l'OCR.
 * Correspondant aux modèles C# du microservice Konta.Ocr.
 */

export enum JobStatus {
    Pending = "Pending",
    Processing = "Processing",
    Completed = "Completed",
    Failed = "Failed"
}

export enum DocumentType {
    Unknown = "Unknown",
    Invoice = "Invoice",
    ExpenseReport = "ExpenseReport",
    Rib = "Rib"
}

/**
 * Représente un job d'extraction OCR.
 */
export interface ExtractionJob {
    id: string;
    tenantId: string;
    fileName: string;
    status: JobStatus;
    detectedType: DocumentType;
    errorMessage?: string;
    createdAt: string;
    processedAt?: string;
}

/**
 * Données extraites d'une facture.
 */
export interface ExtractedInvoice {
    id: string;
    jobId: string;
    vendorName?: string;
    invoiceNumber?: string;
    invoiceDate?: string;
    totalAmountHt?: number;
    totalAmountTtc?: number;
    vatAmount?: number;
    currency?: string;
    rawJson?: string;
}

/**
 * Données extraites d'un RIB.
 */
export interface ExtractedRib {
    id: string;
    jobId: string;
    iban?: string;
    bic?: string;
    bankName?: string;
    accountHolder?: string;
}
