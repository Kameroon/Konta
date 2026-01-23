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
        const errorMessage = response.data.message || 'Impossible de récupérer les résultats de la facture';
        console.error(`[Documents API] Erreur récupération facture pour Job ${jobId}:`, {
            success: response.data.success,
            message: response.data.message,
            data: response.data.data
        });
        throw new Error(errorMessage);
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
    },

    /**
     * Récupère l'historique de tous les jobs d'extraction du tenant.
     */
    async getJobs(): Promise<ExtractionJob[]> {
        console.log('[Documents API] Récupération de l\'historique des jobs...');
        const response = await http.get<ApiResponse<ExtractionJob[]>>('/api/ocr/jobs');

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        return [];
    },

    /**
     * Récupère uniquement les jobs de l'utilisateur connecté.
     */
    async getMyJobs(): Promise<ExtractionJob[]> {
        console.log('[Documents API] Récupération de l\'historique de l\'utilisateur...');
        const response = await http.get<ApiResponse<ExtractionJob[]>>('/api/ocr/jobs/mine');

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        return [];
    },

    /**
     * Supprime un job d'extraction.
     */
    async deleteJob(jobId: string): Promise<void> {
        console.log(`[Documents API] Suppression du job : ${jobId}`);
        const response = await http.delete<ApiResponse<void>>(`/api/ocr/jobs/${jobId}`);

        if (!response.data.success) {
            throw new Error(response.data.message || 'Échec de la suppression');
        }
    },

    /**
     * Télécharge le fichier PDF associé à un job.
     */
    async downloadDocument(jobId: string, fileName: string): Promise<void> {
        console.log(`[Documents API] Téléchargement du document : ${jobId}`);
        const response = await http.get(`/api/ocr/jobs/${jobId}/download`, {
            responseType: 'blob'
        });

        // Créer un lien temporaire pour déclencher le téléchargement
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', fileName || `document_${jobId}.pdf`);
        document.body.appendChild(link);
        link.click();

        // Nettoyage
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
    }
};
