import http from './http';

export interface NavigationItem {
    id: string;
    label: string;
    path: string;
    icon?: string;
    requiredPermission?: string;
    requiredRole?: string;
    displayOrder: number;
    isVisible: boolean;
}

export const navigationApi = {
    getNavigationItems: async (): Promise<NavigationItem[]> => {
        const response = await http.get<NavigationItem[]>('/api/navigation');
        return response.data;
    },

    updateNavigationItem: async (id: string, data: Partial<NavigationItem>): Promise<void> => {
        await http.put(`/api/navigation/${id}`, data);
    }
};
