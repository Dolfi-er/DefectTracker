import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { apiService } from '../services/api';
import type { DefectDTO, DefectHistoryDTO, CommentDTO, DefectAttachmentDTO } from '../types/api';
import { ArrowLeft, Calendar, User, AlertCircle, FolderOpen, MessageSquare, Edit2, Paperclip, Plus, Trash2, ExternalLink } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';

export function DefectDetailsPage() {
  const { defectId } = useParams<{ defectId: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [defect, setDefect] = useState<DefectDTO | null>(null);
  const [history, setHistory] = useState<DefectHistoryDTO[]>([]);
  const [comments, setComments] = useState<CommentDTO[]>([]);
  const [attachments, setAttachments] = useState<DefectAttachmentDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'details' | 'history' | 'comments' | 'attachments'>('details');
  const [newComment, setNewComment] = useState('');
  const [showAttachmentModal, setShowAttachmentModal] = useState(false);
  const [newAttachment, setNewAttachment] = useState({
    fileName: '',
    filePath: ''
  });

  useEffect(() => {
    if (defectId) {
      loadDefectData();
    }
  }, [defectId]);

  const loadDefectData = async () => {
    try {
      const [defectData, historyData, commentsData, attachmentsData] = await Promise.all([
        apiService.getDefect(Number(defectId)),
        apiService.getDefectHistory(Number(defectId)),
        apiService.getCommentsByDefect(Number(defectId)),
        apiService.getDefectAttachments(Number(defectId)),
      ]);
      setDefect(defectData);
      setHistory(historyData);
      setComments(commentsData);
      setAttachments(attachmentsData);
    } catch (error) {
      console.error('Failed to load defect data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newComment.trim() || !defect) return;

    try {
      await apiService.createComment({
        defectId: defect.defectId,
        userId: user!.userId,
        commentText: newComment,
      });
      setNewComment('');
      await loadDefectData();
    } catch (error) {
      console.error('Failed to add comment:', error);
      alert('Failed to add comment');
    }
  };

  const handleAddAttachment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!defect || !newAttachment.fileName || !newAttachment.filePath) return;

    try {
      await apiService.createDefectAttachment({
        defectId: defect.defectId,
        fileName: newAttachment.fileName,
        filePath: newAttachment.filePath,
        fileSize: 0, // Размер будет рассчитываться на сервере
      });
      setNewAttachment({ fileName: '', filePath: '' });
      setShowAttachmentModal(false);
      await loadDefectData();
    } catch (error) {
      console.error('Failed to add attachment:', error);
      alert('Failed to add attachment');
    }
  };

  const handleDeleteAttachment = async (attachmentId: number) => {
    if (!confirm('Are you sure you want to delete this attachment?')) return;

    try {
      await apiService.deleteDefectAttachment(attachmentId);
      await loadDefectData();
    } catch (error) {
      console.error('Failed to delete attachment:', error);
      alert('Failed to delete attachment');
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getPriorityLabel = (priority: number) => {
    if (priority >= 3) return 'High';
    if (priority >= 2) return 'Medium';
    return 'Low';
  };

  const getPriorityColor = (priority: number) => {
    if (priority >= 3) return 'bg-red-100 text-red-700 border-red-200';
    if (priority >= 2) return 'bg-yellow-100 text-yellow-700 border-yellow-200';
    return 'bg-green-100 text-green-700 border-green-200';
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (!defect) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold text-gray-900 mb-4">Defect not found</h2>
        <button
          onClick={() => navigate('/defects')}
          className="text-blue-600 hover:text-blue-700 font-medium"
        >
          Return to Defects
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <button
            onClick={() => navigate('/defects')}
            className="flex items-center gap-2 text-gray-600 hover:text-gray-900 font-medium"
          >
            <ArrowLeft className="w-5 h-5" />
            Back to Defects
          </button>
          <div className="w-px h-6 bg-gray-300"></div>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">{defect.info.defectName}</h1>
            <p className="text-gray-600 mt-1">Defect #{defect.defectId}</p>
          </div>
        </div>
        <span
          className={`px-4 py-2 rounded-full text-sm font-medium border ${getPriorityColor(
            defect.info.priority
          )}`}
        >
          {getPriorityLabel(defect.info.priority)} Priority
        </span>
      </div>

      {/* Tabs */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200">
        <div className="border-b border-gray-200">
          <nav className="flex -mb-px">
            <button
              onClick={() => setActiveTab('details')}
              className={`py-4 px-6 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'details'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              Details
            </button>
            <button
              onClick={() => setActiveTab('history')}
              className={`py-4 px-6 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'history'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              History
            </button>
            <button
              onClick={() => setActiveTab('comments')}
              className={`py-4 px-6 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'comments'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              Comments ({comments.length})
            </button>
            <button
              onClick={() => setActiveTab('attachments')}
              className={`py-4 px-6 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'attachments'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              Attachments ({attachments.length})
            </button>
          </nav>
        </div>

        <div className="p-6">
          {/* Details Tab */}
          {activeTab === 'details' && (
            <div className="space-y-6">
              <div>
                <h3 className="text-lg font-semibold text-gray-900 mb-3">Description</h3>
                <p className="text-gray-700 bg-gray-50 rounded-lg p-4">
                  {defect.info.defectDescription}
                </p>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-900">Basic Information</h3>
                  
                  <div className="flex items-center gap-3">
                    <FolderOpen className="w-5 h-5 text-gray-400" />
                    <div>
                      <p className="text-sm text-gray-600">Project</p>
                      <p className="font-medium text-gray-900">{defect.projectName}</p>
                    </div>
                  </div>

                  <div className="flex items-center gap-3">
                    <AlertCircle className="w-5 h-5 text-gray-400" />
                    <div>
                      <p className="text-sm text-gray-600">Status</p>
                      <p className="font-medium text-blue-600">{defect.statusName}</p>
                    </div>
                  </div>

                  <div className="flex items-center gap-3">
                    <Calendar className="w-5 h-5 text-gray-400" />
                    <div>
                      <p className="text-sm text-gray-600">Due Date</p>
                      <p className="font-medium text-gray-900">
                        {new Date(defect.info.dueDate).toLocaleDateString()}
                      </p>
                    </div>
                  </div>
                </div>

                <div className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-900">Assignment</h3>
                  
                  <div className="flex items-center gap-3">
                    <User className="w-5 h-5 text-gray-400" />
                    <div>
                      <p className="text-sm text-gray-600">Created By</p>
                      <p className="font-medium text-gray-900">{defect.createdByFio}</p>
                    </div>
                  </div>

                  <div className="flex items-center gap-3">
                    <User className="w-5 h-5 text-gray-400" />
                    <div>
                      <p className="text-sm text-gray-600">Assigned To</p>
                      <p className="font-medium text-gray-900">
                        {defect.responsibleFio || 'Unassigned'}
                      </p>
                    </div>
                  </div>

                  <div className="flex items-center gap-3">
                    <Calendar className="w-5 h-5 text-gray-400" />
                    <div>
                      <p className="text-sm text-gray-600">Created Date</p>
                      <p className="font-medium text-gray-900">
                        {new Date(defect.createdDate).toLocaleString()}
                      </p>
                    </div>
                  </div>

                  <div className="flex items-center gap-3">
                    <Calendar className="w-5 h-5 text-gray-400" />
                    <div>
                      <p className="text-sm text-gray-600">Last Updated</p>
                      <p className="font-medium text-gray-900">
                        {new Date(defect.updatedDate).toLocaleString()}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* History Tab */}
          {activeTab === 'history' && (
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900">Change History</h3>
              {history.length === 0 ? (
                <p className="text-gray-500 text-center py-8">No history records found.</p>
              ) : (
                <div className="space-y-3">
                  {history.map((record) => (
                    <div
                      key={record.historyId}
                      className="flex items-start gap-4 p-4 bg-gray-50 rounded-lg"
                    >
                      <div className="w-2 h-2 bg-blue-500 rounded-full mt-2"></div>
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                          <p className="font-medium text-gray-900">{record.userFio}</p>
                          <span className="text-xs text-gray-500">
                            {new Date(record.changeDate).toLocaleString()}
                          </span>
                        </div>
                        <p className="text-sm text-gray-700">
                          Changed <span className="font-medium">{record.fieldName}</span> from{' '}
                          <span className="text-red-600 line-through">{record.oldValue}</span> to{' '}
                          <span className="text-green-600">{record.newValue}</span>
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {/* Comments Tab */}
          {activeTab === 'comments' && (
            <div className="space-y-6">
              <form onSubmit={handleAddComment} className="bg-gray-50 rounded-lg p-4">
                <h3 className="text-lg font-semibold text-gray-900 mb-3">Add Comment</h3>
                <textarea
                  value={newComment}
                  onChange={(e) => setNewComment(e.target.value)}
                  placeholder="Type your comment here..."
                  rows={4}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <div className="mt-3 flex justify-end">
                  <button
                    type="submit"
                    disabled={!newComment.trim()}
                    className="bg-blue-600 text-white px-6 py-2 rounded-lg font-medium hover:bg-blue-700 transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed"
                  >
                    Post Comment
                  </button>
                </div>
              </form>

              <div className="space-y-4">
                <h3 className="text-lg font-semibold text-gray-900">
                  Comments ({comments.length})
                </h3>
                {comments.length === 0 ? (
                  <p className="text-gray-500 text-center py-8">No comments yet.</p>
                ) : (
                  comments.map((comment) => (
                    <div
                      key={comment.commentId}
                      className="flex items-start gap-4 p-4 bg-white border border-gray-200 rounded-lg"
                    >
                      <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center flex-shrink-0">
                        <MessageSquare className="w-5 h-5 text-blue-600" />
                      </div>
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-2">
                          <p className="font-medium text-gray-900">{comment.userFio}</p>
                          <span className="text-xs text-gray-500">
                            {new Date(comment.createdDate).toLocaleString()}
                          </span>
                        </div>
                        <p className="text-gray-700">{comment.commentText}</p>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </div>
          )}

          {/* Attachments Tab */}
          {activeTab === 'attachments' && (
            <div className="space-y-6">
              <div className="flex justify-between items-center">
                <h3 className="text-lg font-semibold text-gray-900">
                  Attachments ({attachments.length})
                </h3>
                <button
                  onClick={() => setShowAttachmentModal(true)}
                  className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors font-medium"
                >
                  <Plus className="w-4 h-4" />
                  Add Attachment
                </button>
              </div>

              {attachments.length === 0 ? (
                <div className="text-center py-12">
                  <Paperclip className="w-12 h-12 text-gray-400 mx-auto mb-4" />
                  <p className="text-gray-500">No attachments yet.</p>
                  <p className="text-gray-400 text-sm mt-1">
                    Add files from Google Drive or other cloud storage
                  </p>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {attachments.map((attachment) => (
                    <div
                      key={attachment.attachmentId}
                      className="flex items-center justify-between p-4 bg-white border border-gray-200 rounded-lg hover:shadow-md transition-shadow"
                    >
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center flex-shrink-0">
                          <Paperclip className="w-5 h-5 text-blue-600" />
                        </div>
                        <div>
                          <a
                            href={attachment.filePath}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="font-medium text-gray-900 hover:text-blue-600 transition-colors flex items-center gap-1"
                          >
                            {attachment.fileName}
                            <ExternalLink className="w-4 h-4" />
                          </a>
                          <p className="text-sm text-gray-500">
                            {formatFileSize(attachment.fileSize)} •{' '}
                            {new Date(attachment.uploadDate).toLocaleDateString()}
                          </p>
                          {attachment.uploadedByFio && (
                            <p className="text-xs text-gray-400">
                              Uploaded by {attachment.uploadedByFio}
                            </p>
                          )}
                        </div>
                      </div>
                      <button
                        onClick={() => handleDeleteAttachment(attachment.attachmentId)}
                        className="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                        title="Delete attachment"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Add Attachment Modal */}
      {showAttachmentModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h2 className="text-2xl font-bold text-gray-900">Add Attachment</h2>
            </div>
            <form onSubmit={handleAddAttachment} className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  File Name
                </label>
                <input
                  type="text"
                  value={newAttachment.fileName}
                  onChange={(e) => setNewAttachment({...newAttachment, fileName: e.target.value})}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="Enter file name"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Cloud Storage URL
                </label>
                <input
                  type="url"
                  value={newAttachment.filePath}
                  onChange={(e) => setNewAttachment({...newAttachment, filePath: e.target.value})}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="https://drive.google.com/..."
                />
                <p className="text-sm text-gray-500 mt-1">
                  Paste the link from Google Drive or other cloud storage
                </p>
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="submit"
                  className="flex-1 bg-blue-600 text-white py-3 rounded-lg font-medium hover:bg-blue-700 transition-colors"
                >
                  Add Attachment
                </button>
                <button
                  type="button"
                  onClick={() => setShowAttachmentModal(false)}
                  className="flex-1 bg-gray-100 text-gray-700 py-3 rounded-lg font-medium hover:bg-gray-200 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}