import http from './http';
import type { ExtractionJob, ExtractedInvoice, ExtractedRib } from '@/types/document.types';
import type { ApiResponse } from '@/types/common.types';

/**
 * Service API pour la gestion des documents et l'OCR.
 */
export const documentsApi = {
    /**
     * Upload d'un fichier PDF pour extraction OCR.
     */
    async uploadDocument(file: File, tenantId: string): Promise<{ jobId: string }> {
        console.log(`[Documents API] Tentative d'upload du fichier : ${file.name}`);

        const formData = new FormData();
        formData.append('file', file);
        formData.append('tenantId', tenantId);

        const response = await http.post<ApiResponse<{ jobId: string }>>('/api/ocr/upload', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        throw new Error(response.data.message || 'Échec de l\'upload');
    },

    /**
     * Récupère le statut actuel d'un job d'extraction.
     */
    async getJobStatus(jobId: string): Promise<ExtractionJob> {
        const response = await http.get<ApiResponse<ExtractionJob>>(`/api/ocr/jobs/${jobId}`);

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        throw new Error(response.data.message || 'Impossible de récupérer le statut du job');
    },

    /**
     * Récupère les résultats de l'extraction pour une facture.
     */
    async getInvoiceResult(jobId: string): Promise<ExtractedInvoice> {
        const response = await http.get<ApiResponse<ExtractedInvoice>>(`/api/ocr/results/${jobId}`);

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        throw new Error(response.data.message || 'Impossible de récupérer les résultats de la facture');
    },

    /**
     * Récupère les résultats de l'extraction pour un RIB.
     */
    async getRibResult(jobId: string): Promise<ExtractedRib> {
        const response = await http.get<ApiResponse<ExtractedRib>>(`/api/ocr/results/${jobId}`);

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        throw new Error(response.data.message || 'Impossible de récupérer les résultats du RIB');
    }
};
